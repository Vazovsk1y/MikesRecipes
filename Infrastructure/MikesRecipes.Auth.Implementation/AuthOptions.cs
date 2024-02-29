using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation;

public record AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    [Range(1, 5)]
    public required int RefreshTokenLifetimeDaysCount { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required JwtOptions JwtOptions { get; init; }
}

public record JwtOptions
{
    public const string SectionName = nameof(AuthOptions.JwtOptions);

    [Required]
    public required string Audience { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string SecretKey { get; init; }

    [Required]
    [Range(1, 5)]
    public required int SkewMinutesCount { get; init; }

    [Required]
    [Range(1, 20)]
    public required int TokenLifetimeMinutesCount { get; init; }
}