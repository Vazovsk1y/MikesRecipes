using RandomRecipes.Domain.Models;
using RandomRecipes.Domain.Shared;
using RandomRecipes.Services.DTOs;

namespace RandomRecipes.Services;

public interface IRecipeService
{
	Task<Response<RecipesSetDTO>> GetAppropriateAsync(IEnumerable<ProductId> includedProducts, int numberOfOtherProducts = 5);
}
