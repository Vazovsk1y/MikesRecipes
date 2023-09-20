﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandomRecipes.Domain.Models;

namespace RandomRecipes.DAL.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
	public void Configure(EntityTypeBuilder<Product> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(e => e.Id).HasConversion(e => e.Value, e => new ProductId(e));

		builder.Property(e => e.Title).IsRequired();
	}
}