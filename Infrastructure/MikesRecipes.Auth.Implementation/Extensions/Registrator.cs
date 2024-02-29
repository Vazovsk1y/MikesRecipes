using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Services.Implementation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Reflection;
using FluentValidation;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MikesRecipes.Auth.Implementation.Extensions;

public static class Registrator
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IEmailConfirmationsSender, EmailConfirmationsSender>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddIdentityCore<User>(e =>
        {
            e.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            e.Lockout.MaxFailedAccessAttempts = 10;
            e.Lockout.AllowedForNewUsers = true;

            e.User.RequireUniqueEmail = true;

            e.SignIn.RequireConfirmedEmail = true;

            e.Password.RequiredLength = 8;
            e.Password.RequireDigit = false;
            e.Password.RequireNonAlphanumeric = false;
            e.Password.RequireUppercase = false;

            e.ClaimsIdentity.SecurityStampClaimType = nameof(User.SecurityStamp);
            e.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
            e.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.Name;
            e.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            e.ClaimsIdentity.EmailClaimType = JwtRegisteredClaimNames.Email;
        })
        .AddRoles<Role>()
        .AddSignInManager<SignInManager<User>>()
        .AddEntityFrameworkStores<MikesRecipesDbContext>()
        .AddTokenProvider<RefreshTokenProvider>(TokenProviders.RefreshTokenProvider.LoginProvider)
        .AddTokenProvider<AccessTokenProvider>(TokenProviders.AccessTokenProvider.LoginProvider)
        .AddDefaultTokenProviders();

        services
            .AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtOptions.SecretKey));

        services.AddAuthentication(e =>
        {
            e.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            e.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.JwtOptions.Issuer,
                ValidAudience = authOptions.JwtOptions.Audience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.FromMinutes(authOptions.JwtOptions.SkewMinutesCount),
            };
        });

        return services;
    }
}
