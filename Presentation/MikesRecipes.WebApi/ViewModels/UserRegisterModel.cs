using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.ViewModels;

#nullable disable
public record UserRegisterModel(
    [Required]
    string Username,
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
    )
{
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}