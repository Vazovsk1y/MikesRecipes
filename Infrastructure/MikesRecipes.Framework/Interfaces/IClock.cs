namespace MikesRecipes.Framework.Interfaces;

public interface IClock
{
    DateTimeOffset GetDateTimeOffsetUtcNow();

    DateTime GetDateTimeUtcNow();
}
