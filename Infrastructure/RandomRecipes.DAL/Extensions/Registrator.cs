using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RandomRecipes.Data;

namespace RandomRecipes.DAL.Extensions;

public static class Registrator
{
	public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddScoped<IDbInitializer, DbInitializer>()
			.AddDbContext<IApplicationDbContext, RandomRecipesDbContext>(e => e.UseSqlServer(configuration.GetConnectionString("Default")))
			.AddScoped<IDataSeeder, DataSeeder>();
			;

		return services;
	}
}
