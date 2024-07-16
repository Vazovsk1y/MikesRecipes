using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public sealed class User : IdentityUser<Guid>
{
    public IEnumerable<UserRole> Roles { get; init; } = new List<UserRole>();

    public User() 
    {
        Id = Guid.NewGuid();
    }
}