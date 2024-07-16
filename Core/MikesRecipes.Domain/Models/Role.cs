using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public sealed class Role : IdentityRole<Guid>
{
    public IEnumerable<UserRole> Users { get; init; } = new List<UserRole>();

    public Role()
    {
        Id = Guid.NewGuid();
    }
}