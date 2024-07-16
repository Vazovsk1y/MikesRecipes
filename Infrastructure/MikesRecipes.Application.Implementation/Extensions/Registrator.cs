using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Application.Implementation.Validators;

namespace MikesRecipes.Application.Implementation.Extensions;

public static class Registrator
{
    public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssembly(typeof(ByTitleFilterValidator).Assembly);
    }
}
