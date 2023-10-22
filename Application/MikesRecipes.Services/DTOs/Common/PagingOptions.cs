namespace MikesRecipes.Services.DTOs.Common;

public record PagingOptions
{
	public int PageSize { get; }

	public int PageIndex { get; }
	public PagingOptions(int pageIndex, int pageSize)
	{
		if (pageIndex <= 0)
		{
			throw new ArgumentException("Page index must be greater than zero.");
		}

		if (pageSize < 0)
		{
			throw new ArgumentException("Page size must be greater than zero or equal to.");
		}

		PageSize = pageSize;
		PageIndex = pageIndex;
	}
}
