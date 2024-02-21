using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class UserToken : IdentityUserToken<Guid>
{
    public const string RefreshTokenName = "Refresh_token";

    public const string RefreshTokenLoginProviderName = "MikesRecipes";
    public required DateTimeOffset ExpiryDate { get; set; }
}