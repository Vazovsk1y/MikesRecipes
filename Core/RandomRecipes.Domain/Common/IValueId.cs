namespace RandomRecipes.Domain.Common;

public interface IValueId<T> where T : IValueId<T>
{
	Guid Value { get; }

	static abstract T Create();
}
