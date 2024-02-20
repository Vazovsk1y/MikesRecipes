using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.Contracts;

public record RecipeDTO(
	RecipeId Id, 
	string Title, 
	string Url, 
	IReadOnlyCollection<ProductDTO> Products);