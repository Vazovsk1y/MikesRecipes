using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MikesRecipes.DAL.PostgreSQL.Extensions;

public static class Registrator
{
    public static void AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: How to obtain connection string in Production mode?
        services.AddDbContext<MikesRecipesDbContext>(e => e.UseNpgsql(configuration.GetConnectionString("Default")));
    }
}
