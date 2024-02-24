using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Framework.Extensions;

public static class Registrator
{
    public static IServiceCollection AddFramework(this IServiceCollection services)
    {
        services.AddScoped<IClock, Clock>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        return services;
    }
}
