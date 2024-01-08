using Microsoft.EntityFrameworkCore;
using MikesRecipes.DAL.Configurations;
using MikesRecipes.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MikesRecipes.DAL;

public class MikesRecipesDbContext : IdentityDbContext<User, Role, Guid>
{
	public const string DatabaseName = "MikesRecipesDb";

	public DbSet<Product> Products { get; set; }

	public DbSet<Recipe> Recipes { get; set; }

	public DbSet<Ingredient> Ingredients { get; set; }

	public MikesRecipesDbContext(DbContextOptions<MikesRecipesDbContext> dbContextOptions) : base(dbContextOptions) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeConfiguration).Assembly);
	}
}
