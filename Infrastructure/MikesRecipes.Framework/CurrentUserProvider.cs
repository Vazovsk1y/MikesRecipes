using Microsoft.AspNetCore.Http;
using MikesRecipes.Framework.Contracts;
using MikesRecipes.Framework.Interfaces;
using System.Security.Claims;

namespace MikesRecipes.Framework;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated is true;

    public CurrentUser? Get()
    {
        var context = _httpContextAccessor.HttpContext;
        return IsAuthenticated ?
            new CurrentUser(Guid.Parse(context.User.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value), context.User.Claims)
            :
            null;
    }
}