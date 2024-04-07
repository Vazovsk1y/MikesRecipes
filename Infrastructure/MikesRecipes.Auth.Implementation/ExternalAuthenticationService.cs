using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace MikesRecipes.Auth.Implementation;

public class ExternalAuthenticationService : BaseAuthService, IExternalAuthenticationService
{
    public ExternalAuthenticationService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        ICurrentUserProvider currentUserProvider, 
        UserManager<User> userManager, 
        IOptions<AuthOptions> authOptions, 
        MikesRecipesDbContext dbContext, 
        SignInManager<User> signInManager) : base(clock, logger, serviceScopeFactory, currentUserProvider, userManager, authOptions, dbContext, signInManager)
    {
    }

    public async Task<Response<TokensDTO>> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var payload = await GetGooglePayload(idToken);

        User user;
        switch (payload)
        {
            case null:
                return Response.Failure<TokensDTO>(Errors.InvalidGoogleIdToken);
            case Payload when _authOptions.SignIn.RequireConfirmedEmail && !payload.EmailVerified:
                return Response.Failure<TokensDTO>(Errors.ConfirmationRequiredFor(nameof(User.Email)));
            default:
                {
                    var userResult = await GetOrCreateUserBasedOnGooglePayload(payload);
                    if (userResult.IsFailure)
                    {
                        return Response.Failure<TokensDTO>(userResult.Errors);
                    }

                    user = userResult.Value;
                    break;
                }
        }

        string accessToken = await _userManager.GenerateUserTokenAsync(user, AccessTokenProvider.LoginProvider, AccessTokenProvider.Name);
        string refreshTokenValue = await _userManager.GenerateUserTokenAsync(user, RefreshTokenProvider.LoginProvider, RefreshTokenProvider.Name);

        await ModifyOrAddRefreshToken(user, refreshTokenValue, cancellationToken);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshTokenValue);
    }

    private Task<Response<User>> GetOrCreateUserBasedOnGooglePayload(Payload payload)
    {
        throw new NotImplementedException();
    }

    public async Task<Payload?> GetGooglePayload(string idToken)
    {
        Payload? payload = null;
        var validationSettings = new ValidationSettings
        {
            Audience = [_authOptions.External.Google.ClientId],
        };
        try
        {
            payload = await ValidateAsync(idToken, validationSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while validating google id token.");
        }
        return payload;
    }

    private async Task ModifyOrAddRefreshToken(User user, string refreshTokenValue, CancellationToken cancellationToken)
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
}