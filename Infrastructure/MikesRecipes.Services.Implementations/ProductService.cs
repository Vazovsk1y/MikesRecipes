using Microsoft.EntityFrameworkCore;
using MikesRecipes.Data;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.DTOs;

namespace MikesRecipes.Services.Implementations;

internal class ProductService(IApplicationDbContext dbContext) : IProductService
{
	private readonly IApplicationDbContext _dbContext = dbContext;

	public async Task<Response<ProductsSetDTO>> GetAsync(string productTitlePattern, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(productTitlePattern))
		{
			return Response.Failure<ProductsSetDTO>(new Error("Incorrect product title passed."));
		}

		var products = await _dbContext.Products
			.Where(pr => pr.Title.Contains(productTitlePattern))
			.Select(e => new ProductDTO(e.Id, e.Title))
			.ToListAsync(cancellationToken);

		return Response.Success(new ProductsSetDTO(products));
	}
}