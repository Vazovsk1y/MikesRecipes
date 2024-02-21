using MikesRecipes.Domain.Models;
using MikesRecipes.Services.Contracts;
using MikesRecipes.WebApi.ViewModels;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.WebApi.Extensions;

public static class Mapper
{
    public static ByIncludedProductsFilter ToDTO(this ByIncludedProductsFilterModel model)
    {
        return new ByIncludedProductsFilter(model.ProductsIds.Select(e => new ProductId(e)), model.OtherProductsCount);
    }

    public static UserRegisterDTO ToDTO(this UserRegisterModel model)
    {
        return new UserRegisterDTO(model.Username, model.Email, model.Password);
    }

    public static UserLoginDTO ToDTO(this UserLoginModel model)
    {
        return new UserLoginDTO(model.Email, model.Password);
    }
}