namespace RandomRecipes.Data;

public interface IDbInitializer
{
	Task InitializeAsync(CancellationToken cancellationToken = default);

	Task SeedDataAsync(CancellationToken cancellationToken = default);
}