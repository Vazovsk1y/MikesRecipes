using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Auth.Implementation;

public class BaseAuthService : BaseService
{
    protected readonly ICurrentUserProvider _currentUserProvider;
    protected readonly UserManager<User> _userManager;
    protected readonly AuthOptions _authOptions;
    protected readonly MikesRecipesDbContext _dbContext;

    public BaseAuthService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentUserProvider currentUserProvider,
        UserManager<User> userManager,
        IOptions<AuthOptions> authOptions,
        MikesRecipesDbContext dbContext) : base(clock, logger, serviceScopeFactory)
    {
        _currentUserProvider = currentUserProvider;
        _userManager = userManager;
        _authOptions = authOptions.Value;
        _dbContext = dbContext;
    }

    protected async Task<Response<User>> IsAuthenticated()
    {
        var currentUser = _currentUserProvider.Get();
        User? user = _currentUserProvider.IsAuthenticated && currentUser is not null ?
            await _userManager.FindByIdAsync(currentUser.Id.ToString())
            :
            null;

        return user is not null ? user : Response.Failure<User>(Errors.Unauthorized);
    }
}