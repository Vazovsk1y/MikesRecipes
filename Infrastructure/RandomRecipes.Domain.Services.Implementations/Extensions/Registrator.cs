using Microsoft.Extensions.DependencyInjection;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.Domain.Services.Implementations.Extensions;

public static class Registrator
{
	public static IServiceCollection AddDomainServices(this IServiceCollection services)
	{
		services.AddTransient<ICsvParser<Product>, ProductCsvParser>();
		return services;
	}
}
