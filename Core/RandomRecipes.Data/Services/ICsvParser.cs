using RandomRecipes.Domain.Common;

namespace RandomRecipes.Data.Services;

public interface ICsvParser<T> where T : IEntity
{
	IAsyncEnumerable<T> EnumerateAsync(string csvFilePath);

	IEnumerable<T> Enumerate(string csvFilePath);
}
