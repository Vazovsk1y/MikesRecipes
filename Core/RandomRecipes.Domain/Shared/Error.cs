namespace RandomRecipes.Domain.Shared;

public record Error
{
	public static readonly Error None = new (string.Empty);

	public string Text { get; }

	public Error(string text)
	{
		ArgumentException.ThrowIfNullOrEmpty(text);
		Text = text;
	}
}
