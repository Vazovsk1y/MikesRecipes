namespace MikesRecipes.Services.Contracts;

public record EmailDTO(string To, string Subject, string Body, bool IsHtml = true)
{
    public string Purpose { get; init; } = Body;
    public string? ToName { get; init; }
}
