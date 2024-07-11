namespace MikesRecipes.Domain.Common;

public interface IValueId<out T> where T : IValueId<T>
{
	Guid Value { get; }

	static abstract T Create();
}
