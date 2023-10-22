﻿namespace MikesRecipes.Services.DTOs.Common;

public abstract record Page<T> : IPage<T>
{
	public IReadOnlyCollection<T> Items { get; }
	public int PageIndex { get; }

	public int TotalPages { get; }

	public bool HasNextPage => PageIndex < TotalPages;

	public bool HasPreviousPage => PageIndex > 1;
	protected Page(
		IReadOnlyCollection<T> items,
		int totalItemsCount,
		int pageIndex,
		int pageSize)
	{
		Validate(totalItemsCount, pageIndex, pageSize);
		PageIndex = pageIndex;
		TotalPages = totalItemsCount == 0 ? totalItemsCount : (int)Math.Ceiling(totalItemsCount / (double)pageSize);
		Items = items;
	}

	private static void Validate(int totalItemsCount,  int pageIndex, int pageSize)
	{
		if (pageIndex <= 0)
		{
			throw new ArgumentException("Page index must be greater than zero.");
		}

		if (pageSize < 0)
		{
			throw new ArgumentException("Page size must be greater than zero or equal to.");
		}

		if (totalItemsCount < 0)
		{
			throw new ArgumentException("Total items count must be greater than zero or equal to.");
		}
	}
}