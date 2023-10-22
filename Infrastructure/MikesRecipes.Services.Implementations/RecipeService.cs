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
		#region --another implementations--

		#region --Linq #1 +-4 seconds 2 - products, 5 - numberOfOtherProducts --

		//var includedProductsIds = includedProducts.ToList();

		//var result = await _dbContext.Products
		//	.AsNoTracking()
		//	.Where(e => includedProductsIds.Contains(e.Id))
		//		 .Join(
		//		  _dbContext.Ingredients,
		//		  productJoin => productJoin.Id,
		//		  ingredient => ingredient.ProductId,
		//		  (productJoin, ingredient) => new { Ingredient = ingredient }
		//			 )
		//			 .Join(
		//			  _dbContext.Recipes,
		//			  ingredientJoin => ingredientJoin.Ingredient.RecipeId,
		//			  recipe => recipe.Id,
		//			  (ingredientJoin, recipe) => new { Recipe = recipe, ingredientJoin.Ingredient }
		//			 )
		//			 .GroupBy(join => join.Recipe)
		//			 .Where(group =>
		//			 group.Select(join => join.Ingredient.ProductId).Distinct().Count() == includedProductsIds.Count
		//			  &&
		//			 group.Select(join => join.Recipe.Ingredients.Count).All(count => count <= numberOfOtherProducts + includedProductsIds.Count)
		//			)
		//			 .Select(group => group.Key.Id)
		//			 .ToListAsync(cancellationToken);

		//var recipesDtos = await _dbContext.Recipes
		//	.AsNoTracking()
		//	.Include(e => e.Ingredients)
		//	.ThenInclude(e => e.Product)
		//	.Where(e => result.Contains(e.Id))
		//	.Select(e => new RecipeDTO
		//	(
		//		e.Id,
		//		e.Title,
		//		e.Url,
		//		e.Ingredients.Select(ing => new IngredientDTO(ing.RecipeId, ing.ProductId, ing.Product.Title)).ToList()
		//	)).ToListAsync(cancellationToken);


		//return Response.Success(new RecipesSetDTO(recipesDtos));

		#endregion

		#region --Sql #1 +-10 seconds 2 - products, 5 - numberOfOtherProducts--

		//var includedProductsIds = includedProducts.ToList();
		//var productsString = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));

		//var recipeIds = await _dbContext.Recipes
		//	.FromSqlRaw(
		//		$@"SELECT r.Id, r.Title, r.Url
		//        FROM Recipes r
		//        INNER JOIN Ingredients i ON r.Id = i.RecipeId
		//        WHERE i.ProductId IN ({productsString})
		//        GROUP BY r.Id, r.Title, r.Url
		//        HAVING COUNT(DISTINCT i.ProductId) = {includedProductsIds.Count}
		//           AND (
		//               SELECT COUNT(*)
		//               FROM Ingredients i1
		//               WHERE i1.RecipeId = r.Id
		//           ) <= {numberOfOtherProducts + includedProductsIds.Count}"
		//		)
		//	.Select(e => e.Id)
		//	.ToListAsync(cancellationToken);


		//var recipesDtos = await _dbContext.Recipes
		//	.AsNoTracking()
		//	.Include(r => r.Ingredients)
		//	.ThenInclude(i => i.Product)
		//	.Where(r => recipeIds.Contains(r.Id))
		//	.Select(e => new RecipeDTO
		//		(
		//			e.Id,
		//			e.Title,
		//			e.Url,
		//			e.Ingredients.Select(ing => new IngredientDTO(ing.RecipeId, ing.ProductId, ing.Product.Title)).ToList()
		//		))
		//	.ToListAsync(cancellationToken);

		//return Response.Success(new RecipesSetDTO(recipesDtos));

		#endregion

		#region --Sql #3 +-2.5 seconds 2 - products, 5 - numberOfOtherProducts--

		//var includedProductsIds = includedProducts.ToList();
		//var productsString = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));

		//var query = $@" 
		//           SELECT [r].[{nameof(Recipe.Id)}]
		//           FROM [Products] AS [p]
		//           INNER JOIN [Ingredients] AS [i] ON [p].[Id] = [i].[ProductId]
		//           INNER JOIN [Recipes] AS [r] ON [i].[RecipeId] = [r].[Id]
		//           WHERE [p].[Id] IN ({productsString})
		//           GROUP BY [r].[Id]
		//           HAVING COUNT(DISTINCT ([i].[ProductId])) = {includedProductsIds.Count} AND
		//               (
		//                   SELECT COUNT(*)
		//                   FROM [Ingredients] AS [i1]
		//                   WHERE [r].[Id] = [i1].[RecipeId]
		//               ) <= {numberOfOtherProducts + includedProductsIds.Count}";

		//var result = await _dbContext.Database
		//	.SqlQuery<Guid>(FormattableStringFactory.Create(query))
		//	.ToListAsync(cancellationToken);

		//var resultRecipesIds = result.Select(e => new RecipeId(e)).ToList();
		//var recipesDtos = await _dbContext.Recipes
		//	.AsNoTracking()
		//	.Include(r => r.Ingredients)
		//	.ThenInclude(i => i.Product)
		//	.Where(r => resultRecipesIds.Contains(r.Id))
		//	.Select(e => new RecipeDTO
		//		(
		//			e.Id,
		//			e.Title,
		//			e.Url,
		//			e.Ingredients.Select(ing => new IngredientDTO(ing.RecipeId, ing.ProductId, ing.Product.Title)).ToList()
		//		))
		//	.ToListAsync(cancellationToken);

		//return Response.Success(new RecipesSetDTO(recipesDtos));

		#endregion

		#endregion

		#region --Sql #2 +-2.5 seconds 2 - products, 5 - numberOfOtherProducts--

		var includedProductsIds = includedProducts.ToList();
		if (!includedProductsIds.Any())
		{
			return Response.Failure<RecipesPage>(new Error("There is no any included product."));
		}

		string productsIdsRow = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));
		string query = $@"
		           SELECT [r].[{nameof(Recipe.Id)}]
		           FROM [{nameof(_dbContext.Products)}] AS [p]
		           INNER JOIN [{nameof(_dbContext.Ingredients)}] AS [i] ON [p].[{nameof(Product.Id)}] = [i].[{nameof(Ingredient.ProductId)}]
		           INNER JOIN [{nameof(_dbContext.Recipes)}] AS [r] ON [i].[{nameof(Ingredient.RecipeId)}] = [r].[{nameof(Recipe.Id)}]
		           WHERE [p].[{nameof(Product.Id)}] IN ({productsIdsRow})
		           GROUP BY [r].[{nameof(Recipe.Id)}]
		           HAVING COUNT(DISTINCT ([i].[{nameof(Ingredient.ProductId)}])) = {includedProductsIds.Count} AND
		               (
		                   SELECT COUNT(*)
		                   FROM [{nameof(_dbContext.Ingredients)}] AS [i1]
		                   WHERE [r].[{nameof(Recipe.Id)}] = [i1].[{nameof(Ingredient.RecipeId)}]
		               ) <= {includedProductsIds.Count + otherProductsCount}";

		var result = await _dbContext.Recipes
			.FromSqlRaw(query)
			.Select(e => e.Id)
			.ToListAsync(cancellationToken);

		int totalItemsCount = result.Count;
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
			.Where(r => result.Contains(r.Id))
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

		#endregion
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
