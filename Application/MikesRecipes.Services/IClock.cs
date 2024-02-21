namespace MikesRecipes.Services;

public interface IClock
{
	DateTimeOffset GetUtcNow();
}
