using Microsoft.AspNetCore.WebUtilities;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.Domain.Models;
using MikesRecipes.WebApi.ViewModels;
using System.Text;
using MikesRecipes.Application.Contracts;
using MikesRecipes.Application.Contracts.Requests;

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

    public static TokensDTO ToDTO(this RefreshModel model)
    {
        return new TokensDTO(model.ExpiredJwtToken, model.RefreshToken);
    }

    public static ResetPasswordDTO? ToDTO(this ResetPasswordModel model)
    {
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch (FormatException)
        {
            return null;
        }

        return new ResetPasswordDTO(model.Email, decodedToken, model.NewPassword);
    }
}