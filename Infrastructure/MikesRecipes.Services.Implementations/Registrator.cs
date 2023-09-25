using Microsoft.Extensions.DependencyInjection;
using RandomRecipes.Services;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
	public static IServiceCollection AddServices(this IServiceCollection services)
	{
		services.AddScoped<IRecipeService, RecipeService>();

		return services;
	}
}
