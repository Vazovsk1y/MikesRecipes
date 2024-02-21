namespace MikesRecipes.Services.Implementations;

public class Clock : IClock
{
    public DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
