using Microsoft.EntityFrameworkCore;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.Data;

public interface IApplicationDbContext
{
	DbSet<Product> Products { get; set; }

	DbSet<Recipe> Recipes { get; set; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
