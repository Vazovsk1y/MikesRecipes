using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Infrastructure;
using MikesRecipes.DAL.PostgreSQL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Auth.Implementation;

public class BaseAuthService(
    IClock clock,
    ILogger<BaseService> logger,
    IServiceScopeFactory serviceScopeFactory,
    ICurrentUserProvider currentUserProvider,
    UserManager<User> userManager,
    IOptions<AuthOptions> authOptions,
    MikesRecipesDbContext dbContext,
    SignInManager<User> signInManager)
    : BaseService(clock, logger, serviceScopeFactory)
{
    protected readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    protected readonly UserManager<User> _userManager = userManager;
    protected readonly AuthOptions _authOptions = authOptions.Value;
    protected readonly MikesRecipesDbContext _dbContext = dbContext;
    protected readonly SignInManager<User> _signInManager = signInManager;
}