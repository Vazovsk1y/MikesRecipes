using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Auth;
using MikesRecipes.DAL.PostgreSQL;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.Application.Contracts.Responses;
using MikesRecipes.Application.Implementation.Extensions;

namespace MikesRecipes.Application.Implementation;

internal class ProductService(
	IClock clock,
	ILogger<BaseService> logger,
	IServiceScopeFactory serviceScopeFactory,
	MikesRecipesDbContext dbContext,
	ICurrentUserProvider currentUserProvider,
	IAuthenticationState authenticationState)
	: BaseApplicationService(clock, logger, serviceScopeFactory, dbContext, currentUserProvider, authenticationState),
		IProductService
{
	private readonly IAuthenticationState _authenticationState = authenticationState;
	private readonly MikesRecipesDbContext _dbContext = dbContext;

	public async Task<Response<IReadOnlyCollection<ProductDTO>>> GetByTitleFilterAsync(ByTitleFilter filter, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return Response.Failure<IReadOnlyCollection<ProductDTO>>(isAuthenticatedResponse.Errors);
        }

        var validationResult = Validate(filter);
        if (validationResult.IsFailure)
		{
			return Response.Failure<IReadOnlyCollection<ProductDTO>>(validationResult.Errors);
		}

		var products = await _dbContext
			.Products
			.Where(pr => EF.Functions.ILike(pr.Title, $"%{filter.Value}%"))
			.Select(e => e.ToDTO())
			.ToListAsync(cancellationToken);

		return products;
	}
}