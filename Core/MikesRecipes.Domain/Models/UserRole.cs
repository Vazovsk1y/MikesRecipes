using Microsoft.AspNetCore.Identity;

namespace MikesRecipes.Domain.Models;

public class UserRole : IdentityUserRole<Guid>
{
    public User? User { get; set; }

    public Role? Role { get; set; }
}