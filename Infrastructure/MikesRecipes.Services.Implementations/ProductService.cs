using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services.Implementations;

internal class ProductService(MikesRecipesDbContext dbContext) : IProductService
{
	private readonly MikesRecipesDbContext _dbContext = dbContext;

	public async Task<Response<IReadOnlyCollection<ProductDTO>>> GetByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			return Response.Failure<IReadOnlyCollection<ProductDTO>>(new Error("Incorrect search term passed."));
		}

		var products = await _dbContext
			.Products
			.Where(pr => pr.Title.Contains(searchTerm))
			.Select(e => new ProductDTO(e.Id, e.Title))
			.ToListAsync(cancellationToken);

		return products;
	}
}