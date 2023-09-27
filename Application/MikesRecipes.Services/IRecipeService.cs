using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.DTOs;

namespace MikesRecipes.Services;

public interface IRecipeService
{
	Task<Response<RecipesSetDTO>> GetAsync(IEnumerable<ProductId> includedProducts, int otherProductsCount = 5, CancellationToken cancellationToken = default);
}
