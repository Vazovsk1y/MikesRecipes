using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL;

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
		var dbContext = scope.ServiceProvider.GetRequiredService<MikesRecipesDbContext>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<MikesRecipesDbContext>>();
		dbContext.Seed(logger);
	}
}