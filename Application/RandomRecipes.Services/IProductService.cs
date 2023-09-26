using RandomRecipes.Domain.Shared;
using RandomRecipes.Services.DTOs;

namespace RandomRecipes.Services;

public interface IProductService
{
	Task<Response<ProductsSetDTO>> GetAsync(string productTitlePattern, CancellationToken cancellationToken = default);
}