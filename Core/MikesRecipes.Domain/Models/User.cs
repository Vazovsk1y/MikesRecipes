using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class User : IdentityUser<Guid>
{
    public override Guid Id { get => base.Id; }

    public User() 
    {
        Id = Guid.NewGuid();
    }
}

	