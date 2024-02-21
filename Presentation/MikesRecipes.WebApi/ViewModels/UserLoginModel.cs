using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.ViewModels;

public record UserLoginModel([Required][EmailAddress]string Email, [Required]string Password);