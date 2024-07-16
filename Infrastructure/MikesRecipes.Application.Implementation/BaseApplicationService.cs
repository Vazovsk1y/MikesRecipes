using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Auth;
using MikesRecipes.DAL.PostgreSQL;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Application.Implementation;

public abstract class BaseApplicationService(
    IClock clock,
    ILogger<BaseService> logger,
    IServiceScopeFactory serviceScopeFactory,
    MikesRecipesDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IAuthenticationState authenticationState)
    : BaseService(clock, logger, serviceScopeFactory)
{
    protected string Code => $"Application.{GetType().Name}";
}
