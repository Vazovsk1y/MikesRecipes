namespace RandomRecipes.Domain.Common;

public abstract class Entity<T> : IEntity 
	where T : IValueId<T>
{
	public T Id { get; }

	protected Entity()
	{
		Id = T.Create();
	}
}
