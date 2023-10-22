using MikesRecipes.Services.DTOs.Common;

namespace MikesRecipes.Services.DTOs;

public record RecipesPage : Page<RecipeDTO>
{
	public RecipesPage(IReadOnlyCollection<RecipeDTO> items, int totalItemsCount, int pageIndex, int pageSize) : base(items, totalItemsCount, pageIndex, pageSize) 
	{
	}
}


