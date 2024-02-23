namespace MikesRecipes.Services.Implementation;

public record AuthOptions
{
    public const string SectionName = "Auth";
    public required string JwtAudience { get; init; }
    public required string JwtIssuer { get; init; }
    public required string JwtSecretKey { get; init; }
    public required int JwtSkewMinutesCount { get; init; }
    public required int JwtTokenLifetimeMinutesCount { get; init; }
    public required int RefreshTokenLifetimeDaysCount { get; init; }
}