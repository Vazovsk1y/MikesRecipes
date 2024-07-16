using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.Auth.Implementation.Infrastructure;

public record AuthClaimsOptions
{
    public const string SectionName = nameof(AuthOptions.ClaimsIdentity);

    [Required]
    public string RoleClaimType { get; init; } = ClaimTypes.Role;

    [Required]
    public string UserNameClaimType { get; init; } = JwtRegisteredClaimNames.Name;

    [Required]
    public string UserIdClaimType { get; init; } = JwtRegisteredClaimNames.Sub;

    [Required]
    public string EmailClaimType { get; init; } = JwtRegisteredClaimNames.Email;

    [Required]
    public string SecurityStampClaimType { get; init; } = nameof(User.SecurityStamp);

    [Required]
    public string EmailConfirmedClaimType { get; init; } = nameof(User.EmailConfirmed);
}