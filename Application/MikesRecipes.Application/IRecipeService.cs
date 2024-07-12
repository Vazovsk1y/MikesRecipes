using MikesRecipes.Application.Contracts.Common;
using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.Application.Contracts.Responses;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Application;

public interface IRecipeService
{
	Task<Response<RecipesPage>> GetByIncludedProductsAsync(
		ByIncludedProductsFilter filter, 
		PagingOptions pagingOptions, 
		CancellationToken cancellationToken = default);

	Task<Response<RecipesPage>> GetAsync(
		PagingOptions pagingOptions, 
		CancellationToken cancellationToken = default);
}

