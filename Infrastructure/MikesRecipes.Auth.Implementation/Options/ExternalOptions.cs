using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation.Options;

public record ExternalOptions
{
    public const string SectionName = nameof(AuthOptions.External);

    [Required]
    [ValidateObjectMembers]
    public required GoogleOptions Google { get; init; }
}
public record GoogleOptions
{
    public const string SectionName = nameof(ExternalOptions.Google);

    public const string ProviderName = "Google";

    [Required]
    public required string ClientId { get; init; }

    [Required]
    public required string ClientSecret { get; init; }
}