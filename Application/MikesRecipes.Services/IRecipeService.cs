﻿using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Services;

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

