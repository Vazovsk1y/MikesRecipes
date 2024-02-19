using MikesRecipes.Domain.Common;

namespace MikesRecipes.Domain.Models;

#nullable disable

public class Product : Entity<ProductId>
{
	public required string Title { get; init; }

	public Product() : base() { }
}

public record ProductId(Guid Value) : IValueId<ProductId>
{
	public static ProductId Create()
	{
		return new ProductId(Guid.NewGuid());
	}
}
