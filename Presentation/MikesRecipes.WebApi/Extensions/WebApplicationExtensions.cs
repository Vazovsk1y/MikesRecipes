using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;
using MikesRecipes.DAL.Services;

namespace MikesRecipes.WebApi.Extensions;

public static class WebApplicationExtensions
{
	public static void MigrateDatabase(this WebApplication application)
	{
		using var scope = application.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<MikesRecipesDbContext>();
		dbContext.Database.Migrate();
	}

	public static void SeedDatabase(this WebApplication application)
	{
		using var scope = application.Services.CreateScope();
		var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
		seeder.Seed();
	}
}