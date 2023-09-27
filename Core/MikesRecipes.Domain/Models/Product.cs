using MikesRecipes.Domain.Common;

namespace MikesRecipes.Domain.Models;

#nullable disable

public class Product : Entity<ProductId>
{
	public string Title { get; set; }

	public Product() : base() { }
}

public record ProductId(Guid Value) : IValueId<ProductId>
{
	public static ProductId Create()
	{
		return new ProductId(Guid.NewGuid());
	}
}
