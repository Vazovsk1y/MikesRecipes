using RandomRecipes.Domain.Common;
using RandomRecipes.Domain.Shared;

namespace RandomRecipes.Domain.Services;

public interface ICsvParser<T> where T : IEntity
{
	Task<Response<IEnumerable<T>>> ParseAsync(string csvFilePath);
}
