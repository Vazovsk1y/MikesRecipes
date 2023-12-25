using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;
using MikesRecipes.Services.Implementations.Extensions;
using System.Data;

namespace MikesRecipes.Services.Implementations;

internal class RecipeService(MikesRecipesDbContext dbContext) : IRecipeService
{
	private readonly MikesRecipesDbContext _dbContext = dbContext;

	public async Task<Response<RecipesPage>> GetByIncludedProductsAsync(
		IEnumerable<ProductId> includedProducts, 
		int otherProductsCount = 5, 
		PagingOptions? pagingOptions = null, 
		CancellationToken cancellationToken = default)
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

		var filteredRecipes =  _dbContext.Recipes.FromSqlRaw(queryRow);
		int totalRecipesCount = filteredRecipes.Count();

		var result = await _dbContext
			.Recipes
			.FromSqlRaw(queryRow)
			.AsNoTracking()
			.Include(r => r.Ingredients)
			.ThenInclude(i => i.Product)
			.OrderBy(e => e.Title)
			.ApplyPaging(pagingOptions)
			.Select(e => new RecipeDTO
				(
					e.Id,
					e.Title,
					e.Url,
					e.Ingredients.Select(ing => new ProductDTO(ing.ProductId, ing.Product.Title)).ToList()
				))
			.ToListAsync(cancellationToken);

		return Response.Success(new RecipesPage(result, totalRecipesCount, pagingOptions));
	}

	public async Task<Response<RecipesPage>> GetAsync(PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default)
	{
		int totalItemsCount = _dbContext.Recipes.Count();

		var recipesDtos = await _dbContext.Recipes
			.AsNoTracking()
			.Include(r => r.Ingredients)
			.ThenInclude(i => i.Product)
			.OrderBy(e => e.Title)
			.ApplyPaging(pagingOptions)
			.Select(e => new RecipeDTO
				(
					e.Id,
					e.Title,
					e.Url,
					e.Ingredients.Select(ing => new ProductDTO(ing.ProductId, ing.Product.Title)).ToList()
				))
			.ToListAsync(cancellationToken);

		return Response.Success(new RecipesPage(recipesDtos, totalItemsCount, pagingOptions));
	}
}
