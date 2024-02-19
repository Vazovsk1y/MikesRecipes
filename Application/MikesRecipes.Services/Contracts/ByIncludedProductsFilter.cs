using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.Contracts;

public record ByIncludedProductsFilter(IEnumerable<ProductId> ProductIds, int OtherProductsCount = 5);


