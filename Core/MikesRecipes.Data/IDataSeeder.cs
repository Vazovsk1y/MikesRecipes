namespace MikesRecipes.Data;

public interface IDataSeeder
{
	Task SeedDataAsync(CancellationToken cancellationToken = default);
}