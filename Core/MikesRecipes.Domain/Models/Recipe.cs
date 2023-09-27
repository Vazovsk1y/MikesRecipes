using MikesRecipes.Domain.Common;

namespace MikesRecipes.Domain.Models;

#nullable disable

public class Recipe : Entity<RecipeId>
{
	public string Title { get; set; }

	public string Url { get; set; }

	public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

	public Recipe() : base() { }
}

public record RecipeId(Guid Value) : IValueId<RecipeId>
{
	public static RecipeId Create()
	{
		return new RecipeId(Guid.NewGuid());
	}
}
