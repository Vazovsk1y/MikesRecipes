namespace MikesRecipes.Data;

public interface IDbInitializer
{
	Task InitializeAsync(CancellationToken cancellationToken = default);
}
