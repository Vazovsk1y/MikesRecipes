﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using MikesRecipes.Domain.Models;
using MikesRecipes.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MikesRecipes.Services.Implementations.Constants;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IClock, Clock>();

        services.AddIdentity<User, Role>(e =>
        {
            e.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            e.Lockout.MaxFailedAccessAttempts = 10;
            e.Lockout.AllowedForNewUsers = true;

            e.User.RequireUniqueEmail = true;

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
        .AddEntityFrameworkStores<MikesRecipesDbContext>()
        .AddTokenProvider<RefreshTokenProvider>(TokenProviders.RefreshTokenProvider.LoginProvider)
        .AddTokenProvider<AccessTokenProvider>(TokenProviders.AccessTokenProvider.LoginProvider)
        .AddDefaultTokenProviders();

        services.AddAuthenticationWithJwtBearer(configuration);

		return services;
    }

    private static IServiceCollection AddAuthenticationWithJwtBearer(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()!;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecretKey));

        collection
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.JwtIssuer,
                ValidAudience = authOptions.JwtAudience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.FromMinutes(authOptions.JwtSkewMinutesCount),
            };
        });

        return collection;
    }
}
