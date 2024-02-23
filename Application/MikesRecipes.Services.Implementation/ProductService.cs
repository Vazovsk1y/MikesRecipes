using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Implementation.Constants;
using MikesRecipes.Services.Implementation.Extensions;

namespace MikesRecipes.Services.Implementation;

internal class ProductService : BaseService, IProductService
{
    public ProductService(
		IClock clock, 
		ILogger<BaseService> logger, 
		MikesRecipesDbContext dbContext, 
		IServiceScopeFactory serviceScopeFactory) : base(clock, logger, dbContext, serviceScopeFactory)
    {
    }

    public async Task<Response<IReadOnlyCollection<ProductDTO>>> GetByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			return Response.Failure<IReadOnlyCollection<ProductDTO>>(Errors.NullOrWhiteSpaceString("Search term"));
		}

		var products = await _dbContext
			.Products
			.Where(pr => pr.Title.Contains(searchTerm))
			.Select(e => e.ToDTO())
			.ToListAsync(cancellationToken);

		return products;
	}
}