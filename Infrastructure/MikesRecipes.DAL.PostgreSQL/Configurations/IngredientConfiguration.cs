﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL.PostgreSQL.Configurations;

internal class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
	public void Configure(EntityTypeBuilder<Ingredient> builder)
	{
		builder.HasKey(e => new { e.ProductId, e.RecipeId });

		builder.HasOne(e => e.Recipe).WithMany(e => e.Ingredients);
	}
}
