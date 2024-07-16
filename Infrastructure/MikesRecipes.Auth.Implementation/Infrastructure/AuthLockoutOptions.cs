using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Infrastructure;

public record AuthLockoutOptions
{
    public const string SectionName = nameof(AuthOptions.Lockout);

    [Required]
    public bool AllowedForNewUsers { get; init; } = true;

    [Required]
    [Range(5, 15)]
    public int MaxFailedAccessAttempts { get; init; } = 5;

    [Required]
    [Range(typeof(TimeSpan), "0.00:05:00", "0.00:30:00")]
    public TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(10);
}
