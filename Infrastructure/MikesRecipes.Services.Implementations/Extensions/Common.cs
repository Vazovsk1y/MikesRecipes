using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Services.Implementations.Extensions;

internal static class Common
{
	public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> collection, PagingOptions? pagingOptions)
	{
		if (pagingOptions is null)
		{
			return collection;
		}

		return collection
			.Skip((pagingOptions.PageIndex - 1) * pagingOptions.PageSize)
			.Take(pagingOptions.PageSize);
	}
}
