using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class Role : IdentityRole<Guid>
{
    public override Guid Id { get => base.Id; }

    public IEnumerable<UserRole> Users { get; set; } = new List<UserRole>();

    public Role()
    {
        Id = Guid.NewGuid();
    }
}