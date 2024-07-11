using MikesRecipes.Domain.Models;
using MikesRecipes.Application.Contracts;
using MikesRecipes.Application.Contracts.Responses;

namespace MikesRecipes.Services.Implementation.Extensions;

internal static class Mapper
{
    public static RecipeDTO ToDTO(this Recipe recipe)
    {
        return new RecipeDTO(
            recipe.Id.Value,
            recipe.Title,
            recipe.Url,
            recipe.Ingredients.Select(pr => pr.Product.ToDTO()).ToList()
        );
    }

    public static ProductDTO ToDTO(this Product product)
    {
        return new ProductDTO(
            product.Id.Value,
            product.Title
        );
    }
}
