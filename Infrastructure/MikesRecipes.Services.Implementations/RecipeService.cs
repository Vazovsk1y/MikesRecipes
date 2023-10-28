using Microsoft.EntityFrameworkCore;
using MikesRecipes.Data;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.DTOs;
using MikesRecipes.Services.DTOs.Common;
using System.Data;

namespace MikesRecipes.Services.Implementations;

internal class RecipeService(IApplicationDbContext dbContext) : IRecipeService
{
	private readonly IApplicationDbContext _dbContext = dbContext;

	public async Task<Response<RecipesPage>> GetByIncludedProductsAsync(IEnumerable<ProductId> includedProducts, int otherProductsCount = 5, PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default)
	{
		var includedProductsIds = includedProducts.ToList();
		if (!includedProductsIds.Any())
		{
			return Response.Failure<RecipesPage>(new Error("There is no any included product."));
		}

		string productsIdsRow = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));
		string queryRow = $@"
		           SELECT [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
		           FROM [{nameof(_dbContext.Products)}] AS [p]
		           INNER JOIN [{nameof(_dbContext.Ingredients)}] AS [i] ON [p].[{nameof(Product.Id)}] = [i].[{nameof(Ingredient.ProductId)}]
		           INNER JOIN [{nameof(_dbContext.Recipes)}] AS [r] ON [i].[{nameof(Ingredient.RecipeId)}] = [r].[{nameof(Recipe.Id)}]
		           WHERE [p].[{nameof(Product.Id)}] IN ({productsIdsRow})
		           GROUP BY [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
		           HAVING COUNT(DISTINCT ([i].[{nameof(Ingredient.ProductId)}])) = {includedProductsIds.Count} 
                           AND [r].[{nameof(Recipe.IngredientsCount)}] <= {otherProductsCount + includedProductsIds.Count}";

		var query = _dbContext.Recipes
			.FromSqlRaw(queryRow);

		int totalItemsCount = query.Count();
		int pageIndex = pagingOptions is null ? 1 : pagingOptions.PageIndex;
		int pageSize = pagingOptions is null ? totalItemsCount : pagingOptions.PageSize;
		if (pageSize > totalItemsCount)
		{
			pageSize = totalItemsCount;
		}

		var recipesDtos = await query
			.AsNoTracking()
			.Include(r => r.Ingredients)
			.ThenInclude(i => i.Product)
			.OrderBy(e => e.Title)
			.Skip((pageIndex - 1) * pageSize)
			.Take(pageSize)
			.Select(e => new RecipeDTO
				(
					e.Id,
					e.Title,
					e.Url,
					e.Ingredients.Select(ing => new IngredientDTO(ing.RecipeId, ing.ProductId, ing.Product.Title)).ToList()
				))
			.ToListAsync(cancellationToken);

		return Response.Success(new RecipesPage(recipesDtos, totalItemsCount, pageIndex, pageSize));
	}

	public async Task<Response<RecipesPage>> GetAsync(PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default)
	{
		int totalItemsCount = _dbContext.Recipes.Count();
		int pageIndex = pagingOptions is null ? 1 : pagingOptions.PageIndex;
		int pageSize = pagingOptions is null ? totalItemsCount : pagingOptions.PageSize;
		if (pageSize > totalItemsCount)
		{
			pageSize = totalItemsCount;
		}

		var recipesDtos = await _dbContext.Recipes
			.AsNoTracking()
			.Include(r => r.Ingredients)
			.ThenInclude(i => i.Product)
			.OrderBy(e => e.Title)
			.Skip((pageIndex - 1) * pageSize)
			.Take(pageSize)
			.Select(e => new RecipeDTO
				(
					e.Id,
					e.Title,
					e.Url,
					e.Ingredients.Select(ing => new IngredientDTO(ing.RecipeId, ing.ProductId, ing.Product.Title)).ToList()
				))
			.ToListAsync(cancellationToken);

		return Response.Success(new RecipesPage(recipesDtos, totalItemsCount, pageIndex, pageSize));
	}
}
