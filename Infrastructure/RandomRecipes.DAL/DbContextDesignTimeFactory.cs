using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RandomRecipes.DAL;

internal class DbContextDesignTimeFactory : IDesignTimeDbContextFactory<RandomRecipesDbContext>
{
	private const string ConnectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={RandomRecipesDbContext.DatabaseName};Integrated Security=True;Connect Timeout=30;";

	public RandomRecipesDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<RandomRecipesDbContext>();
		optionsBuilder.UseSqlServer(ConnectionString);
		return new RandomRecipesDbContext(optionsBuilder.Options);
	}
}
