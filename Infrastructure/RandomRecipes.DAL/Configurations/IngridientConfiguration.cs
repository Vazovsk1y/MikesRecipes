using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.DAL.Configurations;

internal class IngridientConfiguration : IEntityTypeConfiguration<Ingredient>
{
	public void Configure(EntityTypeBuilder<Ingredient> builder)
	{
		builder.HasKey(e => new { e.ProductId, e.RecipeId });
	}
}
