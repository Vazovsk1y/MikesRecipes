using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandomRecipes.Domain.Enums;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.DAL.Configurations;

internal class IngridientConfiguration : IEntityTypeConfiguration<Ingredient>
{
	public void Configure(EntityTypeBuilder<Ingredient> builder)
	{
		builder.HasKey(e => new { e.ProductId, e.RecipeId });

		builder.ToTable(RandomRecipesDbContext.IngridientsTableName);

		builder.OwnsOne(e => e.RequiredAmount, e =>
		{
			e.Property(e => e.Count).IsRequired();
			e.Property(e => e.AmountType).HasConversion(e => e.ToString(), e => Enum.Parse<AmountType>(e)).IsRequired();
		});
	}
}
