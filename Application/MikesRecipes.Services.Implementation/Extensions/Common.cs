using MikesRecipes.Application.Contracts.Common;

namespace MikesRecipes.Services.Implementation.Extensions;

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
