using RandomRecipes.Domain.ValueObjects;

namespace RandomRecipes.Domain.Models;

#nullable disable

public class Ingredient 
{
	public ProductId ProductId { get; init; }

	public RecipeId RecipeId { get; init; }

	public Product Product { get; set; }

	public Recipe Recipe { get; set; }

	public IngridientAmount RequiredAmount { get; set; }
}
