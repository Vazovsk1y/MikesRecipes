using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Auth.Implementation;

public class BaseAuthService : BaseService
{
    protected readonly ICurrentUserProvider _currentUserProvider;
    protected readonly UserManager<User> _userManager;
    protected readonly AuthOptions _authOptions;
    protected readonly MikesRecipesDbContext _dbContext;
    protected readonly SignInManager<User> _signInManager;

    public BaseAuthService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentUserProvider currentUserProvider,
        UserManager<User> userManager,
        IOptions<AuthOptions> authOptions,
        MikesRecipesDbContext dbContext,
        SignInManager<User> signInManager) : base(clock, logger, serviceScopeFactory)
    {
        _currentUserProvider = currentUserProvider;
        _userManager = userManager;
        _authOptions = authOptions.Value;
        _dbContext = dbContext;
        _signInManager = signInManager;
    }
}