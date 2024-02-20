using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MikesRecipes.DAL;

internal class DbContextDesignTimeFactory : IDesignTimeDbContextFactory<MikesRecipesDbContext>
{
    public const string DesignTimeDatabaseName = "MikesRecipesDb";

    private const string ConnectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={DesignTimeDatabaseName};Integrated Security=True;Connect Timeout=30;";

	public MikesRecipesDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<MikesRecipesDbContext>();
		optionsBuilder.UseSqlServer(ConnectionString);
		return new MikesRecipesDbContext(optionsBuilder.Options);
	}
}
