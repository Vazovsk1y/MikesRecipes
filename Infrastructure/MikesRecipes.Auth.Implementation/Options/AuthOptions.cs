using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Options;

public record AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    [ValidateObjectMembers]
    public required AuthUserOptions User { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required AuthClaimsOptions ClaimsIdentity { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required AuthPasswordOptions Password { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required AuthLockoutOptions Lockout { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required AuthSignInOptions SignIn { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required AuthTokensOptions Tokens { get; init; }
}