using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Auth;
using MikesRecipes.DAL;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Services.Implementation;

public abstract class BaseApplicationService : BaseService
{
    protected readonly MikesRecipesDbContext _dbContext;
    protected readonly ICurrentUserProvider _currentUserProvider;
    protected readonly IAuthenticationState _authenticationState;

    protected BaseApplicationService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        MikesRecipesDbContext dbContext,
        ICurrentUserProvider currentUserProvider,
        IAuthenticationState authenticationState) : base(clock, logger, serviceScopeFactory)
    {
        _dbContext = dbContext;
        _currentUserProvider = currentUserProvider;
        _authenticationState = authenticationState;
    }
}
