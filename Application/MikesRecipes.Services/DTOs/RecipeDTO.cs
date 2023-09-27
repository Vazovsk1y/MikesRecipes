using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.DTOs;

public record RecipeDTO(
	RecipeId RecipeId, 
	string Title, 
	string Url, 
	IReadOnlyCollection<IngredientDTO> Ingredients);