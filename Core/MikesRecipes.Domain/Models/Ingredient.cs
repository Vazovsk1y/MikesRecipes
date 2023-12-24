﻿namespace MikesRecipes.Domain.Models;

#nullable disable

public class Ingredient 
{
	public required ProductId ProductId { get; init; }

	public required RecipeId RecipeId { get; init; }

	public Product Product { get; set; }

	public Recipe Recipe { get; set; }
}
