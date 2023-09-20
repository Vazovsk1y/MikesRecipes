using RandomRecipes.Domain.Common;
using RandomRecipes.Domain.Shared;

namespace RandomRecipes.Domain.Services;

public interface ICsvParser<T> where T : IEntity
{
	IAsyncEnumerable<T> EnumerateAsync(string csvFilePath);

	IEnumerable<T> Enumerate(string csvFilePath);
}
