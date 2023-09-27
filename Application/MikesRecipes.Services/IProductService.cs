using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.DTOs;

namespace MikesRecipes.Services;

public interface IProductService
{
	Task<Response<ProductsSetDTO>> GetAsync(string productTitlePattern, CancellationToken cancellationToken = default);
}