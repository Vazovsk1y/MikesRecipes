using MikesRecipes.Domain.Common;

namespace MikesRecipes.Domain.Models;

#nullable disable

public class Recipe : Entity<RecipeId>
{
	public required string Title { get; init; }

	public required string Url { get; init; }

	// Denormalized field to improve sql query response. 
	public int IngredientsCount { get; set; }

	public ICollection<Ingredient> Ingredients { get; init; } = new List<Ingredient>();

	public Recipe() : base() { }
}

public record RecipeId(Guid Value) : IValueId<RecipeId>
{
	public static RecipeId Create()
	{
		return new RecipeId(Guid.NewGuid());
	}
}
