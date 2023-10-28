using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Data;

namespace MikesRecipes.DAL.Extensions;

public static class Registrator
{
	public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddScoped<IDbInitializer, DbInitializer>()
			.AddDbContext<IApplicationDbContext, MikesRecipesDbContext>(e => e.UseSqlServer(configuration.GetConnectionString("Default")))
			.AddScoped<IDataSeeder, DataSeeder>();
			;

		return services;
	}
}
