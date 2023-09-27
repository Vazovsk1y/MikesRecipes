using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL.Configurations;
using MikesRecipes.Data;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL;

public class RandomRecipesDbContext : DbContext, IApplicationDbContext
{
	public const string DatabaseName = "RandomRecipesDb";

	public DbSet<Product> Products { get; set; }

	public DbSet<Recipe> Recipes { get; set; }

	public DbSet<Ingredient> Ingredients { get; set; }

	public RandomRecipesDbContext(DbContextOptions<RandomRecipesDbContext> dbContextOptions) : base(dbContextOptions) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeConfiguration).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
