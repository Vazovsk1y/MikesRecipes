using RandomRecipes.Domain.Models;
using RandomRecipes.Domain.Shared;
using RandomRecipes.Services.DTOs;

namespace RandomRecipes.Services;

public interface IRecipeService
{
	Task<Response<RecipesSetDTO>> GetAsync(IEnumerable<ProductId> includedProducts, int otherProductsCount = 5, CancellationToken cancellationToken = default);
}
