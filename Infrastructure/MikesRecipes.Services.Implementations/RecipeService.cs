using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;
using MikesRecipes.Services.Implementations.Extensions;
using System.Data;

namespace MikesRecipes.Services.Implementations;

internal class RecipeService(
    MikesRecipesDbContext dbContext, 
    IValidator<ByIncludedProductsFilter> validator) : IRecipeService
{
    private readonly MikesRecipesDbContext _dbContext = dbContext;
    private readonly IValidator<ByIncludedProductsFilter> _validator = validator;
    public async Task<Response<RecipesPage>> GetAsync(PagingOptions? pagingOptions = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int totalItemsCount = _dbContext.Recipes.Count();
        var recipesDtos = await _dbContext
            .Recipes
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

    public async Task<Response<RecipesPage>> GetByIncludedProductsAsync(
        ByIncludedProductsFilter filter,
        PagingOptions? pagingOptions = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = _validator.Validate(filter);
        if (!validationResult.IsValid)
        {
            return Response.Failure<RecipesPage>(new Error(validationResult.ToString()));
        }

        string productsIdsRaw = string.Join(",", filter.ProductIds.Select(e => $"'{e.Value}'"));
        string sql = $@"
             SELECT [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
             FROM [{nameof(_dbContext.Products)}] AS [p]
             INNER JOIN [{nameof(_dbContext.Ingredients)}] AS [i] ON [p].[{nameof(Product.Id)}] = [i].[{nameof(Ingredient.ProductId)}]
             INNER JOIN [{nameof(_dbContext.Recipes)}] AS [r] ON [i].[{nameof(Ingredient.RecipeId)}] = [r].[{nameof(Recipe.Id)}]
             WHERE [p].[{nameof(Product.Id)}] IN ({productsIdsRaw})
             GROUP BY [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
             HAVING COUNT(DISTINCT ([i].[{nameof(Ingredient.ProductId)}])) = {filter.ProductIds.Count()} 
                           AND [r].[{nameof(Recipe.IngredientsCount)}] <= {filter.OtherProductsCount + filter.ProductIds.Count()}";

        int totalRecipesCount = _dbContext.Recipes.FromSqlRaw(sql).Count();
        var result = await _dbContext
            .Recipes
            .FromSqlRaw(sql)
            .AsNoTracking()
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
}
