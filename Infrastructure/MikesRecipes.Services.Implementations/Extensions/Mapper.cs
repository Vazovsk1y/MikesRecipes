using MikesRecipes.Domain.Models;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services.Implementations.Extensions;

internal static class Mapper
{
    public static RecipeDTO ToDTO(this Recipe recipe)
    {
        return new RecipeDTO(
            recipe.Id,
            recipe.Title,
            recipe.Url,
            recipe.Ingredients.Select(pr => pr.Product.ToDTO()).ToList()
        );
    }

    public static ProductDTO ToDTO(this Product product)
    {
        return new ProductDTO(
            product.Id,
            product.Title
        );
    }
}
