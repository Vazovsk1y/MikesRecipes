using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		return services;
    }
}
