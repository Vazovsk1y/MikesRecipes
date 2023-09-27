using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL.Configurations;

internal class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
	public void Configure(EntityTypeBuilder<Recipe> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id).HasConversion(e => e.Value, e => new RecipeId(e));

		builder.Property(e => e.Title).IsRequired();

		builder.Property(e => e.Url).IsRequired();

		builder.HasMany(e => e.Ingredients).WithOne(e => e.Recipe).HasForeignKey(e => e.RecipeId);
	}
}
