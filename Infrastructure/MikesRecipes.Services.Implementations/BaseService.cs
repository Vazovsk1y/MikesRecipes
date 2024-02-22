using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Services.Implementations;

public abstract class BaseService
{
    protected readonly IClock _clock;
    protected readonly ILogger _logger;
    protected readonly MikesRecipesDbContext _dbContext;
    protected readonly IServiceScopeFactory _serviceScopeFactory;

    protected BaseService(
        IClock clock, 
        ILogger<BaseService> logger, 
        MikesRecipesDbContext dbContext, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected Response Validate<T>(T @object) where T : notnull
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<T>>();

        var validationResult = validator.Validate(@object);
        if (!validationResult.IsValid)
        {
            return Response.Failure(new Error(validationResult.ToString()));
        }

        return Response.Success();
    }
}
