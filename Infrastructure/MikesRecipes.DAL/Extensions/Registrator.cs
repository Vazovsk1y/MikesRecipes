using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.DAL.Services;

namespace MikesRecipes.DAL.Extensions;

public static class Registrator
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContext<MikesRecipesDbContext>(e => e.UseSqlServer(configuration.GetConnectionString("Default")))
            .AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        ;

        return services;
    }
}
