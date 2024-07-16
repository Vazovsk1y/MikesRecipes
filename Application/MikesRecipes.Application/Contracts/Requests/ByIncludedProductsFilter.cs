using MikesRecipes.Domain.Models;

namespace MikesRecipes.Application.Contracts.Requests;

public record ByIncludedProductsFilter(IEnumerable<ProductId> IncludedProducts, int OtherProductsCount = 5);