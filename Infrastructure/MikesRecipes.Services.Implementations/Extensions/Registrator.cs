using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using MikesRecipes.Domain.Models;
using MikesRecipes.DAL;

namespace MikesRecipes.Services.Implementations;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IAuthProvider, AuthProvider>();

        services.AddIdentity<User, Role>(e =>
        {
            e.SignIn.RequireConfirmedPhoneNumber = false;
            e.SignIn.RequireConfirmedEmail = false;

            e.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            e.Lockout.MaxFailedAccessAttempts = 5;
            e.Lockout.AllowedForNewUsers = true;

            e.User.RequireUniqueEmail = true;

            e.Password.RequiredLength = 8;
            e.Password.RequireDigit = false;
            e.Password.RequireNonAlphanumeric = false;
            e.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<MikesRecipesDbContext>()
        .AddDefaultTokenProviders();

		return services;
    }
}
