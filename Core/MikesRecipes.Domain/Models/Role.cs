using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class Role : IdentityRole<Guid>
{
    public override Guid Id { get => base.Id; }

    public Role()
    {
        Id = Guid.NewGuid();
    }
}