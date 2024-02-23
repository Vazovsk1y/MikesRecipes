using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class UserToken : IdentityUserToken<Guid>
{
    public required DateTimeOffset ExpiryDate { get; set; }
}