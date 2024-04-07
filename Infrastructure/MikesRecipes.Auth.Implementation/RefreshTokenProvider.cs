using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using System.Security.Cryptography;

namespace MikesRecipes.Auth.Implementation;

public class RefreshTokenProvider : IUserTwoFactorTokenProvider<User>
{
    public const string Name = "Refresh_token";

    public const string LoginProvider = "MikesRecipes";

    private readonly ILogger<BaseService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RefreshTokenProvider(
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory) 
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(true);
    }

    public Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(GenerateRandomString());
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        if (purpose != Name)
        {
            throw new InvalidOperationException("Invalid purpose for refresh token.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MikesRecipesDbContext>();

        var refreshToken = await dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == LoginProvider
            && e.Name == purpose);

        var currentDate = clock.GetDateTimeOffsetUtcNow();
        if (refreshToken is null || refreshToken.Value != token || refreshToken.ExpiryDate < currentDate)
        {
            return false;
        }

        return true;
    }

    private static string GenerateRandomString()
    {
        var bytes = new byte[64];
        using var randomGenerator = RandomNumberGenerator.Create();
        randomGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
