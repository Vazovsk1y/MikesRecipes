using MikesRecipes.Domain.Models;

namespace MikesRecipes.Services.DTOs;

public record IngredientDTO(RecipeId RecipeId, ProductId ProductId, string ProductTitle);
