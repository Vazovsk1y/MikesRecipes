namespace MikesRecipes.Services.DTOs.Common;

public interface IPage<T>
{
	IReadOnlyCollection<T> Items { get; }
	int PageIndex { get; }
	int TotalPages { get; }
	bool HasNextPage { get; }
	bool HasPreviousPage { get; }
}
