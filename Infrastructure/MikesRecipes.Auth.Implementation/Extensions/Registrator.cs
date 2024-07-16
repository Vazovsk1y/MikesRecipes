using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.DAL.PostgreSQL;
using MikesRecipes.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using MikesRecipes.Auth.Implementation.Infrastructure;

namespace MikesRecipes.Auth.Implementation.Extensions;

public static class Registrator
{
    public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthenticationProvider>();
        services.AddScoped<IAuthenticationService>(e => e.GetRequiredService<AuthenticationProvider>());
        services.AddScoped<IAuthenticationState>(e => e.GetRequiredService<AuthenticationProvider>());

        services.AddScoped<IEmailConfirmationsSender, EmailConfirmationsSender>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services
            .AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TODO: How to obtain auth options in production mode?
        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()!;
        var identityOptions = authOptions.ToIdentityOptions();

        services
            .AddIdentityCore<User>(e =>
            {
                e.User = identityOptions.User;
                e.SignIn = identityOptions.SignIn;
                e.ClaimsIdentity = identityOptions.ClaimsIdentity;
                e.Password = identityOptions.Password;
                e.Lockout = identityOptions.Lockout;
                e.Tokens = identityOptions.Tokens;
                e.Stores = identityOptions.Stores;
            })
            .AddRoles<Role>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<MikesRecipesDbContext>()
            .AddClaimsPrincipalFactory<AuthUserClaimsPrincipalFactory>()
            .AddTokenProvider<RefreshTokenProvider>(RefreshTokenProvider.LoginProvider)
            .AddTokenProvider<AccessTokenProvider>(AccessTokenProvider.LoginProvider)
            .AddDefaultTokenProviders();

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Tokens.Jwt.SecretKey));

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
                ValidIssuer = authOptions.Tokens.Jwt.Issuer,
                ValidAudience = authOptions.Tokens.Jwt.Audience,
                IssuerSigningKey = signingKey,
                ClockSkew = authOptions.Tokens.Jwt.ClockSkew,
            };
        });
    }
}