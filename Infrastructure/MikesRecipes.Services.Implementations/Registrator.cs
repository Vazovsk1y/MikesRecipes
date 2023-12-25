using Microsoft.Extensions.DependencyInjection;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
