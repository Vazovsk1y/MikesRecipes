using RandomRecipes.Domain.Models;

namespace RandomRecipes.Services.DTOs;

public record RecipeDTO(RecipeId RecipeId, string Title, string Url, IEnumerable<IngredientDTO> Ingredients);