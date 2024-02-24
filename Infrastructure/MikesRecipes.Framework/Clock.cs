using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Framework;

public class Clock : TimeProvider, IClock
{
    public DateTimeOffset GetDateTimeOffsetUtcNow()
    {
        return GetUtcNow();
    }

    public DateTime GetDateTimeUtcNow()
    {
        return GetUtcNow().UtcDateTime;
    }
}
