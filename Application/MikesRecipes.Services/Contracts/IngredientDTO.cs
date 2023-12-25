using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.Contracts;

public record IngredientDTO(RecipeId RecipeId, ProductId ProductId, string ProductTitle);
