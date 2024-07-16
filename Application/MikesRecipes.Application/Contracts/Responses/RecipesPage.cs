using MikesRecipes.Application.Contracts.Common;

namespace MikesRecipes.Application.Contracts.Responses;

public record RecipesPage : Page<RecipeDTO>
{
	public RecipesPage(
		IReadOnlyCollection<RecipeDTO> recipes,
		int totalRecipesCount,
		PagingOptions? pagingOptions = null) : base(recipes,
		totalRecipesCount,
		pagingOptions) 
	{
	}
}

