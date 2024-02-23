using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        using var scope = _serviceScopeFactory.CreateScope();
        var userClaimsPrincipalFactory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<User>>();

        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        var claimsPrincipal = await userClaimsPrincipalFactory.CreateAsync(user);
        return GenerateJwtAccessToken(claimsPrincipal.Claims);
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(token);

        var claimsPrincipal = GetClaimsPrincipalFromJwtToken(token);
        return Task.FromResult(claimsPrincipal is not null && user.Id == Guid.Parse(claimsPrincipal.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value));
    }

    private string GenerateJwtAccessToken(IEnumerable<Claim> claims)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var expiryDate = clock.GetUtcNow().AddMinutes(_authOptions.JwtTokenLifetimeMinutesCount);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtSecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        var token = new JwtSecurityToken(
            _authOptions.JwtIssuer,
            _authOptions.JwtAudience,
            claims,
            null,
            expiryDate.DateTime,
            signingCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }

    private ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string token)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtSecretKey));

        var tokenValidationParametrs = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _authOptions.JwtIssuer,
            ValidAudience = _authOptions.JwtAudience,
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