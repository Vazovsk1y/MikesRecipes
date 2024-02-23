namespace MikesRecipes.Services.Implementation;

public class Clock : IClock
{
    public DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
