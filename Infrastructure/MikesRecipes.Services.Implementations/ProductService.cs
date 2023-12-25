using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services.Implementations;

internal class ProductService(MikesRecipesDbContext dbContext) : IProductService
{
	private readonly MikesRecipesDbContext _dbContext = dbContext;

	public async Task<Response<IReadOnlyCollection<ProductDTO>>> GetAsync(string productTitlePattern, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(productTitlePattern))
		{
			return Response.Failure<IReadOnlyCollection<ProductDTO>>(new Error("Incorrect product title passed."));
		}

		var products = await _dbContext.Products
			.Where(pr => pr.Title.Contains(productTitlePattern))
			.Select(e => new ProductDTO(e.Id, e.Title))
			.ToListAsync(cancellationToken);

		return products;
	}
}