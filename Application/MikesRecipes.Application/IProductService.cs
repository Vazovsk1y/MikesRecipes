using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.Application.Contracts.Responses;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Application;

public interface IProductService
{
	Task<Response<IReadOnlyCollection<ProductDTO>>> GetByTitleFilterAsync(ByTitleFilter filter, CancellationToken cancellationToken = default);
}