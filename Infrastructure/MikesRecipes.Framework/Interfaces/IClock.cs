namespace MikesRecipes.Framework.Interfaces;

public interface IClock
{
    DateTimeOffset GetUtcNow();
}
