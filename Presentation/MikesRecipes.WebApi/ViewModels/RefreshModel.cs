using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.ViewModels;

public record RefreshModel([Required]string ExpiredJwtToken, [Required]string RefreshToken);