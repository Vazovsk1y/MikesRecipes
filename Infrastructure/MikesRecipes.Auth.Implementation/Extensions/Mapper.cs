using MikesRecipes.Auth.Contracts;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.Auth.Implementation.Extensions;

internal static class Mapper
{
    public static UserProfileDTO ToDTO(this User user)
    {
        return new UserProfileDTO(user.Email!, user.EmailConfirmed, user.UserName!);
    }
}
