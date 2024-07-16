using Microsoft.AspNetCore.Identity;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth.Implementation.Extensions;

public static class IdentityExtensions
{
    public static IEnumerable<Error> ToSharedErrors(this IEnumerable<IdentityError> identityErrors)
    {
        return identityErrors.Select(e => new Error($"{Errors.BaseCode}.{e.Code}", e.Description));
    }
}
