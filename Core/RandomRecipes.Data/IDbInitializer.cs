namespace RandomRecipes.Data;

public interface IDbInitializer
{
	Task InitializeAsync(CancellationToken cancellationToken = default);
}
