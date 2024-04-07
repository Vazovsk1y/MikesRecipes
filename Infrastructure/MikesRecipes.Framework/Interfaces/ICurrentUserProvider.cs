using System.Security.Claims;

namespace MikesRecipes.Framework.Interfaces;

public interface ICurrentUserProvider
{
    ClaimsPrincipal? GetCurrentUser();
}