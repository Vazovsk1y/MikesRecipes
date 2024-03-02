using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.Domain.Models;
using MikesRecipes.Framework.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MikesRecipes.Auth.Implementation;

public class AccessTokenProvider : IUserTwoFactorTokenProvider<User>
{
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

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        if (purpose != TokenProviders.AccessTokenProvider.Name)
        {
            throw new InvalidOperationException($"Invalid access token purpose.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        var claimsPrincipal = GetClaimsPrincipalFromJwtToken(token);
        return Task.FromResult(claimsPrincipal is not null && user.Id == Guid.Parse(claimsPrincipal.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value));
    }

    private string GenerateJwtAccessToken(IEnumerable<Claim> claims)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var expiryDate = clock.GetDateTimeUtcNow().Add(_authOptions.Tokens.Jwt.TokenLifetime);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Tokens.Jwt.SecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        var token = new JwtSecurityToken(
            _authOptions.Tokens.Jwt.Issuer,
            _authOptions.Tokens.Jwt.Audience,
            claims,
            null,
            expiryDate,
            signingCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }

    private ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string token)
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
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, tokenValidationParametrs, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when jwt access token validating.");
            return null;
        }
    }
}