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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MikesRecipes.Auth.Implementation.Options;

namespace MikesRecipes.Auth.Implementation.Extensions;

public static class Registrator
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IEmailConfirmationsSender, EmailConfirmationsSender>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services
            .AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()!;
        var identityOptions = authOptions.ToIdentityOptions();

        services
            .AddTransient(e =>
            {
                return Microsoft.Extensions.Options.Options.Create(identityOptions);
            })
            .AddIdentityCore<User>()
            .AddRoles<Role>()
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<MikesRecipesDbContext>()
            .AddTokenProvider<RefreshTokenProvider>(TokenProviders.RefreshTokenProvider.LoginProvider)
            .AddTokenProvider<AccessTokenProvider>(TokenProviders.AccessTokenProvider.LoginProvider)
            .AddClaimsPrincipalFactory<AuthUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Tokens.Jwt.SecretKey));

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

        return services;
    }
}