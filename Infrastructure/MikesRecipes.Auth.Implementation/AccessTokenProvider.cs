using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.Domain.Models;
using MikesRecipes.Framework.Interfaces;
using System.Security.Claims;
using System.Text;

namespace MikesRecipes.Auth.Implementation;

public class AccessTokenProvider : IUserTwoFactorTokenProvider<User>
{
    public const string Name = "Access_token";

    public const string LoginProvider = "Jwt";

    private readonly AuthOptions _authOptions;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AccessTokenProvider(
        ILogger<AccessTokenProvider> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AuthOptions> authOptions)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _authOptions = authOptions.Value;
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(true);
    }

    public async Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        using var scope = _serviceScopeFactory.CreateScope();
        var userClaimsPrincipalFactory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<User>>();

        var claimsPrincipal = await userClaimsPrincipalFactory.CreateAsync(user);
        return GenerateJwtAccessToken(claimsPrincipal.Claims);
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        if (purpose != Name)
        {
            throw new InvalidOperationException($"Invalid access token purpose.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var claimsPrincipal = await GetClaimsPrincipalFromJwtToken(token);
        if (claimsPrincipal is null)
        {
            return false;
        }

        string? userId = manager.GetUserId(claimsPrincipal);
        return !string.IsNullOrWhiteSpace(userId) && Guid.Parse(userId) == user.Id;
    }

    private string GenerateJwtAccessToken(IEnumerable<Claim> claims)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var currentDate = clock.GetDateTimeUtcNow();
        var expiryDate = currentDate.Add(_authOptions.Tokens.Jwt.TokenLifetime);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Tokens.Jwt.SecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _authOptions.Tokens.Jwt.Issuer,
            Audience = _authOptions.Tokens.Jwt.Audience,
            Claims = claims.ToDictionary(e => e.Type, e => (object)e.Value),
            Expires = expiryDate,
            SigningCredentials = signingCredentials,
            NotBefore = currentDate,
            IssuedAt = currentDate,
        };

        string tokenValue = new JsonWebTokenHandler().CreateToken(descriptor);
        return tokenValue;
    }

    private async Task<ClaimsPrincipal?> GetClaimsPrincipalFromJwtToken(string token)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Tokens.Jwt.SecretKey));
        var tokenValidationParametrs = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _authOptions.Tokens.Jwt.Issuer,
            ValidAudience = _authOptions.Tokens.Jwt.Audience,
            IssuerSigningKey = signingKey,
        };

        try
        {
            var handler = new JsonWebTokenHandler();
            var validationResult = await handler.ValidateTokenAsync(token, tokenValidationParametrs);
            return validationResult.IsValid ? new ClaimsPrincipal(validationResult.ClaimsIdentity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when jwt access token validating.");
            return null;
        }
    }
}