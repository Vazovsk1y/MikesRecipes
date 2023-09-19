using RandomRecipes.Domain.Common;
using RandomRecipes.Domain.Shared;

namespace RandomRecipes.Domain.Services;

internal interface ICsvParser<T> where T : IEntity
{
	Task<Response<IEnumerable<T>>> Parse(string csvFilePath);
}
