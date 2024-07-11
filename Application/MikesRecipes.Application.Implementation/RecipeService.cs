using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Auth;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Application.Contracts.Common;
using MikesRecipes.Application;
using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.Application.Contracts.Responses;
using MikesRecipes.Application.Implementation.Extensions;

namespace MikesRecipes.Application.Implementation;

public class RecipeService(
    IClock clock,
    ILogger<BaseService> logger,
    IServiceScopeFactory serviceScopeFactory,
    MikesRecipesDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IAuthenticationState authenticationState)
    : BaseApplicationService(clock, logger, serviceScopeFactory, dbContext, currentUserProvider, authenticationState),
        IRecipeService
{
    private readonly IAuthenticationState _authenticationState = authenticationState;
    private readonly MikesRecipesDbContext _dbContext = dbContext;

    public async Task<Response<RecipesPage>> GetAsync(PagingOptions pagingOptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return Response.Failure<RecipesPage>(isAuthenticatedResponse.Errors);
        }

        var validationResult = Validate(pagingOptions);
        if (validationResult.IsFailure)
        {
            return Response.Failure<RecipesPage>(validationResult.Errors);
        }

        var totalItemsCount = _dbContext.Recipes.Count();
        var recipes = await _dbContext
            .Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .ThenInclude(i => i.Product)
            .OrderBy(e => e.Title)
            .ApplyPaging(pagingOptions)
            .Select(e => e.ToDTO())
            .ToListAsync(cancellationToken);

        return Response.Success(new RecipesPage(recipes, totalItemsCount, pagingOptions));
    }

    public async Task<Response<RecipesPage>> GetByIncludedProductsAsync(
        ByIncludedProductsFilter filter,
        PagingOptions pagingOptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return Response.Failure<RecipesPage>(isAuthenticatedResponse.Errors);
        }

        var validationResult = Validate(pagingOptions, filter);
        if (validationResult.IsFailure)
        {
            return Response.Failure<RecipesPage>(validationResult.Errors);
        }

        var includedProductsIds = filter.IncludedProducts.ToList();
        var includedProductsCount = includedProductsIds.Count;

        var productsIdsRow = string.Join(",", includedProductsIds.Select(e => $"'{e.Value}'"));
        
        // TODO: Rewrite it for PostgreSQL.
        var sql = $"""
                   
                                SELECT [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
                                FROM [{nameof(_dbContext.Products)}] AS [p]
                                INNER JOIN [{nameof(_dbContext.Ingredients)}] AS [i] ON [p].[{nameof(Product.Id)}] = [i].[{nameof(Ingredient.ProductId)}]
                                INNER JOIN [{nameof(_dbContext.Recipes)}] AS [r] ON [i].[{nameof(Ingredient.RecipeId)}] = [r].[{nameof(Recipe.Id)}]
                                WHERE [p].[{nameof(Product.Id)}] IN ({productsIdsRow})
                                GROUP BY [r].[{nameof(Recipe.Id)}], [r].[{nameof(Recipe.Title)}], [r].[{nameof(Recipe.Url)}], [r].[{nameof(Recipe.IngredientsCount)}]
                                HAVING COUNT(DISTINCT ([i].[{nameof(Ingredient.ProductId)}])) = {includedProductsCount} 
                                              AND [r].[{nameof(Recipe.IngredientsCount)}] <= {filter.OtherProductsCount + includedProductsCount}
                   """;

        var totalRecipesCount = _dbContext.Recipes.FromSqlRaw(sql).Count();
        var result = await _dbContext
            .Recipes
            .FromSqlRaw(sql)
            .AsNoTracking()
            .Include(e => e.Ingredients)
            .ThenInclude(e => e.Product)
            .OrderBy(e => e.Title)
            .ApplyPaging(pagingOptions)
            .Select(e => e.ToDTO())
            .ToListAsync(cancellationToken);

        return Response.Success(new RecipesPage(result, totalRecipesCount, pagingOptions));
    }
}
