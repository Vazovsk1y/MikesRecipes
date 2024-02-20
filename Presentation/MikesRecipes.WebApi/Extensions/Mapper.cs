using MikesRecipes.Domain.Models;
using MikesRecipes.Services.Contracts;
using MikesRecipes.WebApi.ViewModels;

namespace MikesRecipes.WebApi.Extensions;

public static class Mapper
{
    public static ByIncludedProductsFilter ToDTO(this ByIncludedProductsFilterModel model)
    {
        return new ByIncludedProductsFilter(model.ProductsIds.Select(e => new ProductId(e)), model.OtherProductsCount);
    }
}