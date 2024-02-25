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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MikesRecipes.Auth.Implementation;

public class AuthProvider : BaseService, IAuthProvider
{
    private readonly UserManager<User> _userManager;
    private readonly AuthOptions _authOptions;
    private readonly MikesRecipesDbContext _dbContext;
    private readonly IUserConfirmation<User> _confirmation;

    public AuthProvider(
        IClock clock,
        ILogger<BaseService> logger,
        MikesRecipesDbContext dbContext,
        IServiceScopeFactory serviceScopeFactory,
        UserManager<User> userManager,
        IOptions<AuthOptions> authOptions,
        IUserConfirmation<User> confirmation) : base(clock, logger, serviceScopeFactory)
    {
        _userManager = userManager;
        _authOptions = authOptions.Value;
        _dbContext = dbContext;
        _confirmation = confirmation;
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
            var creationResult = await _userManager.CreateAsync(user, userRegisterDTO.Password);
            if (!creationResult.Succeeded)
            {
                return Response.Failure(creationResult.Errors.Select(e => new Error(e.Code, e.Description)));
            }

            var addedUser = await _userManager.FindByEmailAsync(user.Email);
            await _userManager.AddToRoleAsync(user, DefaultRoles.User);
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

        var validationResult = Validate(userLoginDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure<TokensDTO>(validationResult.Errors);
        }

        var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
        if (user is null)
        {
            return Response.Failure<TokensDTO>(Errors.InvalidEmailOrPassword);
        }

        var canLoginResult = await CanLogin(user, userLoginDTO.Password);
        if (canLoginResult.IsFailure)
        {
            return Response.Failure<TokensDTO>(canLoginResult.Errors);
        }

        string accessToken = await _userManager.GenerateUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name);
        string refreshToken = await _userManager.GenerateUserTokenAsync(user, TokenProviders.RefreshTokenProvider.LoginProvider, TokenProviders.RefreshTokenProvider.Name);

        var existingRefreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == TokenProviders.RefreshTokenProvider.LoginProvider
            && e.Name == TokenProviders.RefreshTokenProvider.Name, cancellationToken);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        if (existingRefreshToken is not null)
        {
            existingRefreshToken.ExpiryDate = currentDate.AddDays(_authOptions.RefreshTokenLifetimeDaysCount);
            existingRefreshToken.Value = refreshToken;
        }
        else
        {
            var token = new UserToken
            {
                ExpiryDate = currentDate.AddDays(_authOptions.RefreshTokenLifetimeDaysCount),
                Value = refreshToken,
                UserId = user.Id,
                LoginProvider = TokenProviders.RefreshTokenProvider.LoginProvider,
                Name = TokenProviders.RefreshTokenProvider.Name
            };

            _dbContext.UserTokens.Add(token);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshToken);
    }

    public async Task<Response<string>> RefreshTokenAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationResult = Validate(tokensDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure<string>(validationResult.Errors);
        }

        var userId = GetUserIdFromJwtToken(tokensDTO.AccessToken);
        var user = userId is null ? null : await _userManager.FindByIdAsync(userId.ToString()!);
        bool accessTokenVerificationResult = user is not null 
            && await _userManager.VerifyUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name, tokensDTO.AccessToken);

        if (userId is null || user is null || !accessTokenVerificationResult)
        {
            return Response.Failure<string>(Errors.InvalidAccessToken);
        }

        bool refreshTokenVerificationResult = await _userManager.VerifyUserTokenAsync(
            user, 
            TokenProviders.RefreshTokenProvider.LoginProvider, 
            TokenProviders.RefreshTokenProvider.Name, 
            tokensDTO.RefreshToken);

        if (!refreshTokenVerificationResult)
        {
            return Response.Failure<string>(Errors.InvalidRefreshToken);
        }

        string newAccessToken = await _userManager.GenerateUserTokenAsync(user, TokenProviders.AccessTokenProvider.LoginProvider, TokenProviders.AccessTokenProvider.Name);
        return newAccessToken;
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
        catch(Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while getting user id from jwt token.");
            return null;
        }
    }

    private async Task<Response> CanLogin(User user, string password)
    {
        if (_userManager.Options.SignIn.RequireConfirmedEmail && !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return Response.Failure(Errors.ConfirmationRequiredFor(nameof(User.Email)));
        }

        if (_userManager.Options.SignIn.RequireConfirmedPhoneNumber && !(await _userManager.IsPhoneNumberConfirmedAsync(user)))
        {
            return Response.Failure(Errors.ConfirmationRequiredFor(nameof(User.PhoneNumber)));
        }

        if (_userManager.Options.SignIn.RequireConfirmedAccount && !(await _confirmation.IsConfirmedAsync(_userManager, user)))
        {
            return Response.Failure(Errors.ConfirmationRequiredFor("Account"));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Response.Failure<TokensDTO>(Errors.UserLockedOut);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);
            return Response.Failure<TokensDTO>(Errors.InvalidEmailOrPassword);
        }

        return Response.Success();
    }
}
