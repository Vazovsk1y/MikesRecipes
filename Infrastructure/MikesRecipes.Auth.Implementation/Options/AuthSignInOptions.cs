using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Options;

public record AuthSignInOptions
{
    public const string SectionName = nameof(AuthOptions.SignIn);

    [Required]
    public bool RequireConfirmedEmail { get; init; } = true;

    [Required]
    public bool RequireConfirmedPhoneNumber { get; init; } = false;

    [Required]
    public bool RequireConfirmedAccount { get; init; } = false;
}
