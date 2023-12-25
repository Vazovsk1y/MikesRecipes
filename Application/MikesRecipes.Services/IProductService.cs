using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services;

public interface IProductService
{
	Task<Response<IReadOnlyCollection<ProductDTO>>> GetAsync(string productTitleSearchTerm, CancellationToken cancellationToken = default);
}