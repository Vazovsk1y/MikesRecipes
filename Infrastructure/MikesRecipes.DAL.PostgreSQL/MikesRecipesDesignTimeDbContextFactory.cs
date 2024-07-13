using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MikesRecipes.DAL.PostgreSQL;

internal class MikesRecipesDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MikesRecipesDbContext>
{
    private const string ConnectionString = "User ID=postgres;Password=12345678;Host=localhost;Port=5432;Database=MikesRecipesDb;";

	public MikesRecipesDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<MikesRecipesDbContext>();
		optionsBuilder.UseNpgsql(ConnectionString);
		return new MikesRecipesDbContext(optionsBuilder.Options);
	}
}
