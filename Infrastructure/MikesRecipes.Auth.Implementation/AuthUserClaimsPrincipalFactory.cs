using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MikesRecipes.Domain.Models;
using System.Security.Claims;
using MikesRecipes.Auth.Implementation.Infrastructure;

namespace MikesRecipes.Auth.Implementation;

public class AuthUserClaimsPrincipalFactory(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    IOptions<AuthOptions> authOptionsAccessor)
    : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, optionsAccessor)
{
    private readonly AuthClaimsOptions _authClaimsIdentityOptions = authOptionsAccessor.Value.ClaimsIdentity;

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var baseId = await base.GenerateClaimsAsync(user);
        baseId.AddClaim(new Claim(_authClaimsIdentityOptions.EmailConfirmedClaimType, user.EmailConfirmed.ToString()));

        return new ClaimsIdentity(baseId.Claims, JwtBearerDefaults.AuthenticationScheme); ;
    }
}