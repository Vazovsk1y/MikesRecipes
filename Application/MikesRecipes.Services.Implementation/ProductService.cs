using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Application;
using MikesRecipes.Auth;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Application.Contracts;
using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.Application.Contracts.Responses;
using MikesRecipes.Services.Implementation.Constants;
using MikesRecipes.Services.Implementation.Extensions;

namespace MikesRecipes.Services.Implementation;

internal class ProductService : BaseApplicationService, IProductService
{
    public ProductService(
		IClock clock, 
		ILogger<BaseService> logger, 
		IServiceScopeFactory serviceScopeFactory,
		MikesRecipesDbContext dbContext, 
		ICurrentUserProvider currentUserProvider,
		IAuthenticationState authenticationState) : base(clock, logger, serviceScopeFactory, dbContext, currentUserProvider, authenticationState)
    {
    }

    public async Task<Response<IReadOnlyCollection<ProductDTO>>> GetByTitleAsync(ByTitleFilter filter, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return Response.Failure<IReadOnlyCollection<ProductDTO>>(isAuthenticatedResponse.Errors);
        }

        if (string.IsNullOrWhiteSpace(filter.Value))
		{
			return Response.Failure<IReadOnlyCollection<ProductDTO>>(new Error("ff", "ffff"));
		}

		var products = await _dbContext
			.Products
			.Where(pr => pr.Title.Contains(filter.Value))
			.Select(e => e.ToDTO())
			.ToListAsync(cancellationToken);

		return products;
	}
}