using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Framework;

public class Clock : IClock
{
    public DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
