using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.ViewModels;

public record ResetPasswordModel([Required][EmailAddress]string Email, [Required]string Token, [Required]string NewPassword)
{
    [Compare(nameof(NewPassword))]
    public required string ConfirmPassword { get; init; }
}