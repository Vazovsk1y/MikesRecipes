using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Services;

public interface IRecipeService
{
	Task<Response<RecipesPage>> GetByIncludedProductsAsync(IEnumerable<ProductId> includedProducts, int otherProductsCount = 5, PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default);

	Task<Response<RecipesPage>> GetAsync(PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default);
}

