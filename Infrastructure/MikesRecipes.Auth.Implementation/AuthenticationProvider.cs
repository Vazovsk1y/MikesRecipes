using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Extensions;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;
using MikesRecipes.Auth.Contracts.Requests;
using MikesRecipes.Auth.Contracts.Responses;

namespace MikesRecipes.Auth.Implementation;

public class AuthenticationProvider : 
    BaseAuthService, 
    IAuthenticationService, 
    IAuthenticationState
{
    private readonly IUserConfirmation<User> _confirmation;
    private readonly IEmailConfirmationsSender _emailConfirmationsSender;
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    private readonly AuthSignInOptions _signInOptions;

    public AuthenticationProvider(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentUserProvider currentUserProvider,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<AuthOptions> authOptions,
        MikesRecipesDbContext dbContext,
        IUserConfirmation<User> confirmation,
        IEmailConfirmationsSender emailConfirmationsSender) : base(clock, logger, serviceScopeFactory, currentUserProvider, userManager, authOptions, dbContext, signInManager)
    {
        _confirmation = confirmation;
        _emailConfirmationsSender = emailConfirmationsSender;
        _signInOptions = _authOptions.SignIn;
    }

    public async Task<Response<User>> IsAuthenticatedAsync(bool checkEmail = true, bool checkSecurityStamp = true, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || currentUser.Identity?.IsAuthenticated is false)
        {
            return Response.Failure<User>(Errors.Unauthorized);
        }

        var user = checkSecurityStamp ? 
            await _signInManager.ValidateSecurityStampAsync(currentUser)
            :
            await _userManager.GetUserAsync(currentUser);

        if (user is null || (checkEmail && _signInOptions.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user)))
        {
            return Response.Failure<User>(Errors.Unauthorized);
        }

        return user;
    }

    public async Task<Response> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_signInOptions.RequireConfirmedEmail)
        {
            return Response.Success();
        }

        if (!EmailAddressAttribute.IsValid(email) || await _userManager.FindByEmailAsync(email) is not { } user)
        {
            return Response.Failure(Errors.InvalidEmailOrPassword);
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            return Response.Failure(Errors.EmailIsAlreadyConfirmed);
        }

        var sendingEmailConfirmationLinkResponse = await _emailConfirmationsSender.SendEmailConfirmationLinkAsync(user, cancellationToken);
        return sendingEmailConfirmationLinkResponse.IsSuccess ? Response.Success() : sendingEmailConfirmationLinkResponse;
    }

    public async Task<Response> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return isAuthenticatedResponse;
        }

        User user = isAuthenticatedResponse.Value;
        var refreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == RefreshTokenProvider.LoginProvider
            && e.Name == RefreshTokenProvider.Name, cancellationToken);

        if (refreshToken is not null)
        {
            _dbContext.UserTokens.Remove(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Response.Success();
    }

    public async Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(userRegisterDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure(validationResult.Errors);
        }

        var user = new User
        {
            Email = userRegisterDTO.Email.Trim(),
            UserName = userRegisterDTO.Username.Trim(),
        };

        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var userCreationResult = await _userManager.CreateAsync(user, userRegisterDTO.Password);
            if (!userCreationResult.Succeeded)
            {
                return Response.Failure(userCreationResult.Errors.ToErrors());
            }

            var addingToRoleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.User);
            if (!addingToRoleResult.Succeeded)
            {
                return Response.Failure(addingToRoleResult.Errors.ToErrors());
            }

            if (_signInOptions.RequireConfirmedEmail)
            {
                var sendEmailConfirmationLinkResponse = await _emailConfirmationsSender.SendEmailConfirmationLinkAsync(user, cancellationToken);
                if (sendEmailConfirmationLinkResponse.IsFailure)
                {
                    return sendEmailConfirmationLinkResponse;
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Something went wrong during user registration.");
            return Response.Failure(Errors.RegistrationFailed);
        }

        return Response.Success();
    }

    public async Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var canLoginResult = await CanLogin(userLoginDTO);
        if (canLoginResult.IsFailure)
        {
            return Response.Failure<TokensDTO>(canLoginResult.Errors);
        }

        User user = canLoginResult.Value;
        string accessToken = await _userManager.GenerateUserTokenAsync(user, AccessTokenProvider.LoginProvider, AccessTokenProvider.Name);
        string refreshTokenValue = await _userManager.GenerateUserTokenAsync(user, RefreshTokenProvider.LoginProvider, RefreshTokenProvider.Name);

        await ModifyOrCreateRefreshToken(user, refreshTokenValue, cancellationToken);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshTokenValue);
    }

    private async Task ModifyOrCreateRefreshToken(User user, string refreshTokenValue, CancellationToken cancellationToken)
    {
        var refreshToken = await _dbContext
                    .UserTokens
                    .SingleOrDefaultAsync(e => e.UserId == user.Id
                    && e.LoginProvider == RefreshTokenProvider.LoginProvider
                    && e.Name == RefreshTokenProvider.Name, cancellationToken);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();

        if (refreshToken is null)
        {
            refreshToken = new UserToken
            {
                ExpiryDate = currentDate.Add(_authOptions.Tokens.Refresh.TokenLifetime),
                Value = refreshTokenValue,
                UserId = user.Id,
                LoginProvider = RefreshTokenProvider.LoginProvider,
                Name = RefreshTokenProvider.Name
            };

            _dbContext.UserTokens.Add(refreshToken);
        }
        else
        {
            refreshToken.ExpiryDate = currentDate.Add(_authOptions.Tokens.Refresh.TokenLifetime);
            refreshToken.Value = refreshTokenValue;
        }
    }

    public async Task<Response<TokensDTO>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return Response.Failure<TokensDTO>(isAuthenticatedResponse.Errors);
        }

        User user = isAuthenticatedResponse.Value;
        bool refreshTokenVerificationResult = await _userManager.VerifyUserTokenAsync(
            user, 
            RefreshTokenProvider.LoginProvider,
            RefreshTokenProvider.Name,
            refreshToken);

        if (!refreshTokenVerificationResult)
        {
            return Response.Failure<TokensDTO>(Errors.InvalidRefreshToken);
        }

        var existingRefreshToken = await _dbContext
           .UserTokens
           .SingleAsync(e => 
           e.UserId == user.Id
           && e.LoginProvider == RefreshTokenProvider.LoginProvider
           && e.Name == RefreshTokenProvider.Name, cancellationToken);

        string newRefreshTokenValue = await _userManager.GenerateUserTokenAsync(user, RefreshTokenProvider.LoginProvider, RefreshTokenProvider.Name);
        string newAccessToken = await _userManager.GenerateUserTokenAsync(user, AccessTokenProvider.LoginProvider, AccessTokenProvider.Name);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        existingRefreshToken.ExpiryDate = currentDate.Add(_authOptions.Tokens.Refresh.TokenLifetime);
        existingRefreshToken.Value = newRefreshTokenValue;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(newAccessToken, newRefreshTokenValue);
    }

    public async Task<Response> ConfirmEmailAsync(EmailConfirmationDTO emailConfirmationDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_signInOptions.RequireConfirmedEmail)
        {
            return Response.Failure(Errors.EmailConfirmationDisabled);
        }

        var validationResult = Validate(emailConfirmationDTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var user = await _userManager.FindByIdAsync(emailConfirmationDTO.UserId.ToString());
        if (user is null)
        {
            return Response.Failure(Errors.UserNotFound);
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            return Response.Failure(Errors.EmailIsAlreadyConfirmed);
        }

        var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationDTO.DecodedToken);
        return result.Succeeded ? Response.Success() : Response.Failure(result.Errors.ToErrors());
    }

    public async Task<Response> ConfirmEmailChangeAsync(EmailChangeConfirmationDTO emailChangeConfirmationDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_signInOptions.RequireConfirmedEmail)
        {
            return Response.Failure(Errors.EmailConfirmationDisabled);
        }

        var validationResult = Validate(emailChangeConfirmationDTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var user = await _userManager.FindByIdAsync(emailChangeConfirmationDTO.UserId.ToString());
        if (user is null)
        {
            return Response.Failure(Errors.UserNotFound);
        }

        string newEmail = emailChangeConfirmationDTO.NewEmail.Trim();
        if (user.Email == newEmail && await _userManager.IsEmailConfirmedAsync(user))
        {
            return Response.Failure(Errors.EmailIsAlreadyConfirmed);
        }

        var result = await _userManager.ChangeEmailAsync(user, newEmail, emailChangeConfirmationDTO.DecodedToken);
        return result.Succeeded ? Response.Success() : Response.Failure(result.Errors.ToErrors());
    }

    private async Task<Response<User>> CanLogin(UserLoginDTO userLoginDTO)
    {
        var validationResult = Validate(userLoginDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure<User>(validationResult.Errors);
        }

        var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
        if (user is null)
        {
            return Response.Failure<User>(Errors.InvalidEmailOrPassword);
        }

        if (_signInOptions.RequireConfirmedEmail && !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return Response.Failure<User>(Errors.ConfirmationRequiredFor(nameof(User.Email)));
        }

        if (_signInOptions.RequireConfirmedPhoneNumber && !(await _userManager.IsPhoneNumberConfirmedAsync(user)))
        {
            return Response.Failure<User>(Errors.ConfirmationRequiredFor(nameof(User.PhoneNumber)));
        }

        if (_signInOptions.RequireConfirmedAccount && !(await _confirmation.IsConfirmedAsync(_userManager, user)))
        {
            return Response.Failure<User>(Errors.ConfirmationRequiredFor("Account"));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Response.Failure<User>(Errors.UserLockedOut);
        }

        if (!await _userManager.CheckPasswordAsync(user, userLoginDTO.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Response.Failure<User>(Errors.InvalidEmailOrPassword);
        }

        return user;
    }
}
