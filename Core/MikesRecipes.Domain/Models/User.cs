using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

#nullable disable
public class User : IdentityUser<Guid>
{
    public override Guid Id { get => base.Id; }

    public IEnumerable<UserRole> Roles { get; set; } = new List<UserRole>();

    public User() 
    {
        Id = Guid.NewGuid();
    }
}