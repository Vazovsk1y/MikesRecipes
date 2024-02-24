using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.DAL;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Services.Implementation;

public abstract class BaseApplicationService : BaseService
{
    protected readonly MikesRecipesDbContext _dbContext;
    protected readonly ICurrentUserProvider _currentUserProvider;

    protected BaseApplicationService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        MikesRecipesDbContext dbContext,
        ICurrentUserProvider currentUserProvider) : base(clock, logger, serviceScopeFactory)
    {
        _dbContext = dbContext;
        _currentUserProvider = currentUserProvider;
    }
}
