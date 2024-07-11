namespace MikesRecipes.Application.Contracts.Responses;

public record RecipeDTO(
	Guid Id, 
	string Title, 
	string Url, 
	IReadOnlyCollection<ProductDTO> Products);