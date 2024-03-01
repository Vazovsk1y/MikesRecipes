﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MikesRecipes.Auth.Implementation;

public class AuthProvider : BaseService, IAuthProvider
{
    private readonly UserManager<User> _userManager;
    private readonly AuthOptions _authOptions;
    private readonly MikesRecipesDbContext _dbContext;
    private readonly IUserConfirmation<User> _confirmation;
    private readonly IEmailConfirmationsSender _emailConfirmationsSender;
    private readonly ICurrentUserProvider _currentUserProvider;
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    private readonly SignInOptions _signInOptions;

    public AuthProvider(
        IClock clock,
        ILogger<BaseService> logger,
        MikesRecipesDbContext dbContext,
        IServiceScopeFactory serviceScopeFactory,
        UserManager<User> userManager,
        IOptions<AuthOptions> authOptions,
        IUserConfirmation<User> confirmation,
        IEmailConfirmationsSender emailConfirmationsSender,
        ICurrentUserProvider currentUserProvider) : base(clock, logger, serviceScopeFactory)
    {
        _userManager = userManager;
        _authOptions = authOptions.Value;
        _dbContext = dbContext;
        _confirmation = confirmation;
        _emailConfirmationsSender = emailConfirmationsSender;
        _currentUserProvider = currentUserProvider;

        _signInOptions = _userManager.Options.SignIn;
    }

    public async Task<Response> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_userManager.Options.SignIn.RequireConfirmedEmail)
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

        var currentUser = _currentUserProvider.Get();
        User? user = _currentUserProvider.IsAuthenticated && currentUser is not null ?
            await _userManager.FindByIdAsync(currentUser.Id.ToString())
            :
            null;

        if (user is null)
        {
            return Response.Failure(Errors.Unauthorized);
        }

        var refreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == TokenProviders.RefreshTokenProvider.LoginProvider
            && e.Name == TokenProviders.RefreshTokenProvider.Name, cancellationToken);

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
                return Response.Failure(userCreationResult.Errors.Select(e => new Error(e.Code, e.Description)));
            }

            await _userManager.AddToRoleAsync(user, DefaultRoles.User);

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
        string accessToken = await _userManager.GenerateUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name);
        string refreshTokenValue = await _userManager.GenerateUserTokenAsync(user, TokenProviders.RefreshTokenProvider.LoginProvider, TokenProviders.RefreshTokenProvider.Name);

        var refreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == TokenProviders.RefreshTokenProvider.LoginProvider
            && e.Name == TokenProviders.RefreshTokenProvider.Name, cancellationToken);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();

        if (refreshToken is null)
        {
            refreshToken = new UserToken
            {
                ExpiryDate = currentDate.AddDays(_authOptions.RefreshTokenLifetimeDaysCount),
                Value = refreshTokenValue,
                UserId = user.Id,
                LoginProvider = TokenProviders.RefreshTokenProvider.LoginProvider,
                Name = TokenProviders.RefreshTokenProvider.Name
            };

            _dbContext.UserTokens.Add(refreshToken);
        }
        else
        {
            refreshToken.ExpiryDate = currentDate.AddDays(_authOptions.RefreshTokenLifetimeDaysCount);
            refreshToken.Value = refreshTokenValue;
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshTokenValue);
    }

    public async Task<Response<TokensDTO>> RefreshTokensAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(tokensDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure<TokensDTO>(validationResult.Errors);
        }

        var userId = GetUserIdFromJwtToken(tokensDTO.AccessToken);
        var user = userId is not null ? await _userManager.FindByIdAsync(userId.ToString()!) : null;
        bool accessTokenVerificationResult = user is not null
            && await _userManager.VerifyUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name, tokensDTO.AccessToken);

        if (user is null || !accessTokenVerificationResult)
        {
            return Response.Failure<TokensDTO>(Errors.InvalidAccessToken);
        }

        bool refreshTokenVerificationResult = await _userManager.VerifyUserTokenAsync(
            user, 
            TokenProviders.RefreshTokenProvider.LoginProvider,
            TokenProviders.RefreshTokenProvider.Name,
            tokensDTO.RefreshToken);

        if (!refreshTokenVerificationResult)
        {
            return Response.Failure<TokensDTO>(Errors.InvalidRefreshToken);
        }

        var existingRefreshToken = await _dbContext
           .UserTokens
           .SingleAsync(e => e.UserId == user.Id
           && e.LoginProvider == TokenProviders.RefreshTokenProvider.LoginProvider
           && e.Name == TokenProviders.RefreshTokenProvider.Name, cancellationToken);

        string newRefreshTokenValue = await _userManager.GenerateUserTokenAsync(user, TokenProviders.RefreshTokenProvider.LoginProvider, TokenProviders.RefreshTokenProvider.Name);
        string newAccessToken = await _userManager.GenerateUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        existingRefreshToken.ExpiryDate = currentDate.AddDays(_authOptions.RefreshTokenLifetimeDaysCount);
        existingRefreshToken.Value = newRefreshTokenValue;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(newAccessToken, newRefreshTokenValue);
    }

    public async Task<Response> ConfirmEmailAsync(EmailConfirmationDTO emailConfirmationDTO, CancellationToken cancellationToken = default)
    {
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
        return result.Succeeded ? Response.Success() : Response.Failure(result.Errors.Select(e => new Error(e.Code, e.Description)));
    }

    public async Task<Response> ConfirmEmailChangeAsync(EmailChangeConfirmationDTO emailChangeConfirmationDTO, CancellationToken cancellationToken = default)
    {
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
        return result.Succeeded ? Response.Success() : Response.Failure(result.Errors.Select(e => new Error(e.Code, e.Description)));
    }

    private Guid? GetUserIdFromJwtToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(token);
            string userIdString = securityToken.Subject ?? securityToken.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value;
            return Guid.Parse(userIdString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while getting user id from jwt token.");
            return null;
        }
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
