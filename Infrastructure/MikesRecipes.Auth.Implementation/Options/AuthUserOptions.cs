using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Options;

public record AuthUserOptions
{
    public const string SectionName = nameof(AuthOptions.User);

    [Required]
    public string AllowedUserNameCharacters { get; init; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    [Required]
    public bool RequireUniqueEmail { get; init; } = true;
}
