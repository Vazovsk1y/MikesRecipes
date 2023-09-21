﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RandomRecipes.DAL;

#nullable disable

namespace RandomRecipes.DAL.Migrations
{
    [DbContext(typeof(RandomRecipesDbContext))]
    partial class RandomRecipesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RandomRecipes.Domain.Models.Ingredient", b =>
                {
                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RecipeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ProductId", "RecipeId");

                    b.HasIndex("RecipeId");

                    b.ToTable("Ingredients", (string)null);
                });

            modelBuilder.Entity("RandomRecipes.Domain.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("RandomRecipes.Domain.Models.Recipe", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Instruction")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("RandomRecipes.Domain.Models.Ingredient", b =>
                {
                    b.HasOne("RandomRecipes.Domain.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RandomRecipes.Domain.Models.Recipe", "Recipe")
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("RandomRecipes.Domain.ValueObjects.IngridientAmount", "RequiredAmount", b1 =>
                        {
                            b1.Property<Guid>("IngredientProductId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("IngredientRecipeId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("AmountType")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<double>("Count")
                                .HasColumnType("float");

                            b1.Property<string>("ExtraInfo")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("IngredientProductId", "IngredientRecipeId");

                            b1.ToTable("Ingredients");

                            b1.WithOwner()
                                .HasForeignKey("IngredientProductId", "IngredientRecipeId");
                        });

                    b.Navigation("Product");

                    b.Navigation("Recipe");

                    b.Navigation("RequiredAmount");
                });

            modelBuilder.Entity("RandomRecipes.Domain.Models.Recipe", b =>
                {
                    b.Navigation("Ingredients");
                });
#pragma warning restore 612, 618
        }
    }
}
