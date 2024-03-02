using Microsoft.AspNetCore.Identity;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth.Implementation.Extensions;

public static class IdentityExtensions
{
    public static IEnumerable<Error> ToErrors(this IEnumerable<IdentityError> identityErrors)
    {
        return identityErrors.Select(e => new Error($"{Errors.CodeBase}.{e.Code}", e.Description));
    }

    public static IdentityOptions ToIdentityOptions(this AuthOptions authOptions)
    {
        return new IdentityOptions
        {
            User = authOptions.User.ToIdentityUserOptions(),
            ClaimsIdentity = authOptions.ClaimsIdentity.ToClaimsIdentityOptions(),
            Password = authOptions.Password.ToIdentityPasswordOptions(),
            Lockout = authOptions.Lockout.ToIdentityLockoutOptions(),
            SignIn = authOptions.SignIn.ToIdentitySignInOptions(),
        };
    }

    private static UserOptions ToIdentityUserOptions(this AuthUserOptions authUserOptions)
    {
        return new UserOptions
        {
            AllowedUserNameCharacters = authUserOptions.AllowedUserNameCharacters,
            RequireUniqueEmail = authUserOptions.RequireUniqueEmail,
        };
    }

    private static ClaimsIdentityOptions ToClaimsIdentityOptions(this AuthClaimsOptions authClaimsOptions)
    {
        return new ClaimsIdentityOptions
        {
            RoleClaimType = authClaimsOptions.RoleClaimType,
            UserNameClaimType = authClaimsOptions.UserNameClaimType,
            UserIdClaimType = authClaimsOptions.UserIdClaimType,
            EmailClaimType = authClaimsOptions.EmailClaimType,
            SecurityStampClaimType = authClaimsOptions.SecurityStampClaimType,
        };
    }

    private static PasswordOptions ToIdentityPasswordOptions(this AuthPasswordOptions authPasswordOptions)
    {
        return new PasswordOptions
        {
            RequiredLength = authPasswordOptions.RequiredLength,
            RequiredUniqueChars = authPasswordOptions.RequiredUniqueChars,
            RequireNonAlphanumeric = authPasswordOptions.RequireNonAlphanumeric,
            RequireLowercase = authPasswordOptions.RequireLowercase,
            RequireUppercase = authPasswordOptions.RequireUppercase,
            RequireDigit = authPasswordOptions.RequireDigit
        };
    }

    private static LockoutOptions ToIdentityLockoutOptions(this AuthLockoutOptions authLockoutOptions)
    {
        var lockoutOptions = new LockoutOptions
        {
            AllowedForNewUsers = authLockoutOptions.AllowedForNewUsers,
            MaxFailedAccessAttempts = authLockoutOptions.MaxFailedAccessAttempts,
            DefaultLockoutTimeSpan = authLockoutOptions.DefaultLockoutTimeSpan
        };
        return lockoutOptions;
    }

    private static SignInOptions ToIdentitySignInOptions(this AuthSignInOptions authSignInOptions)
    {
        var signInOptions = new SignInOptions
        {
            RequireConfirmedEmail = authSignInOptions.RequireConfirmedEmail,
            RequireConfirmedPhoneNumber = authSignInOptions.RequireConfirmedPhoneNumber,
            RequireConfirmedAccount = authSignInOptions.RequireConfirmedAccount
        };
        return signInOptions;
    }
}
