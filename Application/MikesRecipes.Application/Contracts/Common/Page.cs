namespace MikesRecipes.Application.Contracts.Common;

public abstract record Page<T>
{
	private const int StartCountingFrom = 1;

	public IReadOnlyCollection<T> Items { get; }

	public int PageIndex { get; }

	public int TotalCount { get; }

	public int TotalPages { get; }

	public bool HasNextPage => PageIndex < TotalPages;

	public bool HasPreviousPage => PageIndex > StartCountingFrom;

	protected Page(
		IReadOnlyCollection<T> items,
		int totalItemsCount,
		PagingOptions? pagingOptions = null)
	{
		Validate(items, totalItemsCount, pagingOptions);

		Items = items;
		TotalCount = totalItemsCount;
		PageIndex = pagingOptions?.PageIndex ?? StartCountingFrom;
		TotalPages = CalculateTotalPages(pagingOptions, totalItemsCount);
	}

	private static int CalculateTotalPages(PagingOptions? pagingOptions, int totalItemsCount)
	{
		if (pagingOptions is null)
		{
			return StartCountingFrom;
		}

		return totalItemsCount < pagingOptions.PageSize ? StartCountingFrom : (int)Math.Ceiling(totalItemsCount / (double)pagingOptions.PageSize);
	}

	private static void Validate(
		IReadOnlyCollection<T> items,
		int totalItemsCount,
		PagingOptions? pagingOptions)
	{
		switch (pagingOptions)
		{
			case { PageIndex: <= 0 }:
				throw new ArgumentException("Page index must be greater than zero.");
			case { PageSize: <= 0 }:
				throw new ArgumentException("Page size must be greater than zero.");
		}

		if (items.Count > totalItemsCount)
		{
			throw new ArgumentException("Total items count can't be lower than current items count.");
		}

		if (pagingOptions is null && totalItemsCount != items.Count)
		{
			throw new ArgumentException("If paging options is not passed, total items count must be equal to current items count.");
		}
	}
}
