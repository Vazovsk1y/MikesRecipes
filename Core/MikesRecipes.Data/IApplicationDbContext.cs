using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.Data;

public interface IApplicationDbContext
{
	DbSet<Product> Products { get; set; }

	DbSet<Recipe> Recipes { get; set; }

	DbSet<Ingredient> Ingredients { get; set; }

	DatabaseFacade Database { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	int SaveChanges();
}
