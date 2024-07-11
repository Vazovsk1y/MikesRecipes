using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Application;

namespace MikesRecipes.Application.Implementation.Extensions;

public static class Registrator
{
    public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
    }
}
