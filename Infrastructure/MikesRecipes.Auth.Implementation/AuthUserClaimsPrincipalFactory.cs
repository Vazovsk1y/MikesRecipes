using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.Domain.Models;
using System.Security.Claims;

namespace MikesRecipes.Auth.Implementation;

public class AuthUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
{
    private readonly AuthClaimsOptions _authClaimsIdentityOptions;

    public AuthUserClaimsPrincipalFactory(
        UserManager<User> userManager, 
        RoleManager<Role> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        IOptions<AuthOptions> authOptionsAccessor) : base(userManager, roleManager, optionsAccessor)
    {
        _authClaimsIdentityOptions = authOptionsAccessor.Value.ClaimsIdentity;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var baseId = await base.GenerateClaimsAsync(user);
        baseId.AddClaim(new Claim(_authClaimsIdentityOptions.EmailConfirmedClaimType, user.EmailConfirmed.ToString()));

        return new ClaimsIdentity(baseId.Claims, JwtBearerDefaults.AuthenticationScheme); ;
    }
}