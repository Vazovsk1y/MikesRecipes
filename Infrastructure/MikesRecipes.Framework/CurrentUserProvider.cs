using Microsoft.AspNetCore.Http;
using MikesRecipes.Framework.Contracts;
using MikesRecipes.Framework.Interfaces;
using System.Security.Claims;

namespace MikesRecipes.Framework;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly HttpContext _httpContext;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        _httpContext = httpContext;
    }

    public bool IsAuthenticated => _httpContext.User.Identity?.IsAuthenticated is true;

    public CurrentUser? Get()
    {
        return IsAuthenticated ?
            new CurrentUser(Guid.Parse(_httpContext.User.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value), _httpContext.User.Claims)
            :
            null;
    }
}