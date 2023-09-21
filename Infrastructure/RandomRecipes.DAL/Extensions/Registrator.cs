using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RandomRecipes.Data;
using RandomRecipes.Data.Services;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.DAL.Extensions;

public static class Registrator
{
	public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddScoped<IDbInitializer, DbInitializer>()
			.AddDbContext<IApplicationDbContext, RandomRecipesDbContext>(e => e.UseSqlServer(configuration.GetConnectionString("Default")))
			.AddTransient<ICsvParser<Product>, ProductCsvParser>()
			;

		return services;
	}
}
