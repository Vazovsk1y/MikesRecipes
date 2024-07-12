using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.WebApi.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValidateSecurityStampFilterAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        
        if (httpContext.User.Identity is null or { IsAuthenticated: false })
        {
            return;
        }

        using var scope = httpContext.RequestServices.CreateScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();
        var user = await signInManager.ValidateSecurityStampAsync(httpContext.User);
        if (user is null)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}