using Microsoft.AspNetCore.Http;
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

    public ClaimsPrincipal? GetCurrentUser()
    {
        return _httpContext.User;
    }
}