using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace MikesRecipes.Auth.Implementation.Infrastructure;

public record AuthTokensOptions
{
    public const string SectionName = nameof(AuthOptions.Tokens);

    [Required]
    [ValidateObjectMembers]
    public required JwtOptions Jwt { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required RefreshOptions Refresh { get; init; }
}

public record RefreshOptions
{
    public const string SectionName = nameof(AuthTokensOptions.Refresh);

    [Required]
    [Range(typeof(TimeSpan), "0.12:00:00", "10.00:00:00")]
    public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromDays(5);
}

public record JwtOptions
{
    public const string SectionName = nameof(AuthTokensOptions.Jwt);

    [Required]
    public required string Audience { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string SecretKey { get; init; }

    [Required]
    [Range(typeof(TimeSpan), "0.00:01:00", "0.00:05:00")]
    public TimeSpan ClockSkew { get; init; } = TimeSpan.FromMinutes(2);

    [Required]
    [Range(typeof(TimeSpan), "0.00:05:00", "0.00:15:00")]
    public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromMinutes(10);
}