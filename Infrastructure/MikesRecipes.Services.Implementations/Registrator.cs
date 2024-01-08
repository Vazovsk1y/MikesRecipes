using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();

		return services;
    }
}
