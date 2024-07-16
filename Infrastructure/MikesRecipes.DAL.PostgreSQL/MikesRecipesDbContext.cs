using Microsoft.EntityFrameworkCore;
using MikesRecipes.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MikesRecipes.DAL.PostgreSQL.Configurations;
using MikesRecipes.Domain.Constants;

namespace MikesRecipes.DAL.PostgreSQL;

public class MikesRecipesDbContext(DbContextOptions<MikesRecipesDbContext> dbContextOptions) : IdentityDbContext<
	User, Role, Guid,
	IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
	IdentityRoleClaim<Guid>, UserToken>(dbContextOptions)
{
	public DbSet<Product> Products { get; set; }

	public DbSet<Recipe> Recipes { get; set; }

	public DbSet<Ingredient> Ingredients { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeConfiguration).Assembly);
		
		var roles = new Role[] 
		{
			new()
			{
				ConcurrencyStamp = Guid.NewGuid().ToString(),
				Name = DefaultRoles.Admin,
				NormalizedName = DefaultRoles.Admin.ToUpper(),
			},
			new()
			{
				ConcurrencyStamp = Guid.NewGuid().ToString(),
				Name = DefaultRoles.User,
				NormalizedName = DefaultRoles.User.ToUpper(),
			}
		};
		
		modelBuilder.Entity<Role>().HasData(roles);
	}
}
