using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.Domain.Models;
using System.Security.Claims;

namespace MikesRecipes.Auth.Implementation;

public class AuthUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<User>
{
    private readonly UserManager<User> _userManager;
    private readonly AuthClaimsOptions _claimsIdentityOptions;
    public AuthUserClaimsPrincipalFactory(
        UserManager<User> userManager, 
        IOptions<AuthClaimsOptions> optionsAccessor)
    {
        _userManager = userManager;
        _claimsIdentityOptions = optionsAccessor.Value;
    }

    public async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var id = await GenerateClaimsAsync(user).ConfigureAwait(false);
        return new ClaimsPrincipal(id);
    }

    private async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var id = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme, _claimsIdentityOptions.UserNameClaimType, _claimsIdentityOptions.RoleClaimType);

        id.AddClaim(new Claim(_claimsIdentityOptions.UserIdClaimType, user.Id.ToString()));
        id.AddClaim(new Claim(_claimsIdentityOptions.UserNameClaimType, user.UserName!));

        if (_userManager.SupportsUserEmail)
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                id.AddClaim(new Claim(_claimsIdentityOptions.EmailClaimType, user.Email));
                id.AddClaim(new Claim(_claimsIdentityOptions.EmailConfirmedClaimType, user.EmailConfirmed.ToString()));
            }
        }

        if (_userManager.SupportsUserSecurityStamp)
        {
            id.AddClaim(new Claim(_claimsIdentityOptions.SecurityStampClaimType, user.SecurityStamp!));
        }

        if (_userManager.SupportsUserClaim)
        {
            id.AddClaims(await _userManager.GetClaimsAsync(user).ConfigureAwait(false));
        }

        return id;
    }
}