using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;
using MikesRecipes.Services.Implementation.Extensions;
using System.Data;

namespace MikesRecipes.Services.Implementation;

public class RecipeService : BaseApplicationService, IRecipeService
{
    public RecipeService(
        IClock clock, 
        ILogger<BaseService> logger, 
        MikesRecipesDbContext dbContext, 
        IServiceScopeFactory serviceScopeFactory) : base(clock, logger, serviceScopeFactory, dbContext)
    {
    }

    public async Task<Response<RecipesPage>> GetAsync(PagingOptions pagingOptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(pagingOptions);
        if (validationResult.IsFailure)
        {
            return Response.Failure<RecipesPage>(validationResult.Errors);
        }

        int totalItemsCount = _dbContext.Recipes.Count();
        var recipesDtos = await _dbContext
            .Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Product)
            .OrderBy(e => e.Title)
            .ApplyPaging(pagingOptions)
            .Select(e => e.ToDTO())
            .ToListAsync(cancellationToken);

        return Response.Success(new RecipesPage(recipesDtos, totalItemsCount, pagingOptions));
    }

    public async Task<Response<RecipesPage>> GetByIncludedProductsAsync(
        ByIncludedProductsFilter filter,
        PagingOptions pagingOptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(pagingOptions, filter);
        if (validationResult.IsFailure)
        {
            return Response.Failure<RecipesPage>(validationResult.Errors);
        }

        var includedProductsIds = filter.ProductIds.ToList();
        int includedProductsCount = includedProductsIds.Count;

        string productsIdsRaw = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));
        string sql = $@"
             SELECT [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
             FROM [{nameof(_dbContext.Products)}] AS [p]
             INNER JOIN [{nameof(_dbContext.Ingredients)}] AS [i] ON [p].[{nameof(Product.Id)}] = [i].[{nameof(Ingredient.ProductId)}]
             INNER JOIN [{nameof(_dbContext.Recipes)}] AS [r] ON [i].[{nameof(Ingredient.RecipeId)}] = [r].[{nameof(Recipe.Id)}]
             WHERE [p].[{nameof(Product.Id)}] IN ({productsIdsRaw})
             GROUP BY [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
             HAVING COUNT(DISTINCT ([i].[{nameof(Ingredient.ProductId)}])) = {includedProductsCount} 
                           AND [r].[{nameof(Recipe.IngredientsCount)}] <= {filter.OtherProductsCount + includedProductsCount}";

        int totalRecipesCount = _dbContext.Recipes.FromSqlRaw(sql).Count();
        var result = await _dbContext
            .Recipes
            .FromSqlRaw(sql)
            .AsNoTracking()
            .OrderBy(e => e.Title)
            .ApplyPaging(pagingOptions)
            .Select(e => e.ToDTO())
            .ToListAsync(cancellationToken);

        return Response.Success(new RecipesPage(result, totalRecipesCount, pagingOptions));
    }
}
