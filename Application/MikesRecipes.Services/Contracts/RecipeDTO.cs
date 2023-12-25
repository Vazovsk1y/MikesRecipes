using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.Contracts;

public record RecipeDTO(
	RecipeId RecipeId, 
	string Title, 
	string Url, 
	IReadOnlyCollection<ProductDTO> Ingredients);