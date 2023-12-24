using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL.Configurations;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL;

public class MikesRecipesDbContext : DbContext
{
	public const string DatabaseName = "MikesRecipesDb";

	public DbSet<Product> Products { get; set; }

	public DbSet<Recipe> Recipes { get; set; }

	public DbSet<Ingredient> Ingredients { get; set; }

	public MikesRecipesDbContext(DbContextOptions<MikesRecipesDbContext> dbContextOptions) : base(dbContextOptions) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeConfiguration).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
