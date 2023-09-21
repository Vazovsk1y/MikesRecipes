using RandomRecipes.Domain.Models;

namespace RandomRecipes.Services.DTOs;

public record IngredientDTO(RecipeId RecipeId, ProductId ProductId, string ProductTitle);
