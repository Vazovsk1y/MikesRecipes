using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Options;

public record AuthPasswordOptions
{
    public const string SectionName = nameof(AuthOptions.Password);

    [Required]
    [Range(6, 15)]
    public int RequiredLength { get; init; } = 6;

    [Required]
    [Range(1, 4)]
    public int RequiredUniqueChars { get; init; } = 1;

    [Required]
    public bool RequireNonAlphanumeric { get; init; } = false;

    [Required]
    public bool RequireLowercase { get; init; } = true;

    [Required]
    public bool RequireUppercase { get; init; } = false;

    [Required]
    public bool RequireDigit { get; init; } = false;
}
