using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Services.Contracts;

public record RecipesPage : Page<RecipeDTO>
{
	public RecipesPage(IReadOnlyCollection<RecipeDTO> recipes, int totalRecipesCount, PagingOptions? pagingOptions = null): base(recipes, totalRecipesCount, pagingOptions) 
	{
	}
}


