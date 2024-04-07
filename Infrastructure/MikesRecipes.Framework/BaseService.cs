﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Framework;

public abstract class BaseService
{
    protected readonly IClock _clock;
    protected readonly ILogger _logger;
    protected readonly IServiceScopeFactory _serviceScopeFactory;

    protected BaseService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _clock = clock;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected Response Validate<T>(T @object) where T : notnull
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<T>>();

        var validationResult = validator.Validate(@object);
        if (!validationResult.IsValid)
        {
            return Response.Failure(validationResult.Errors.Select(e => new Error(e.ErrorCode, e.ErrorMessage)));
        }

        return Response.Success();
    }

    protected Response Validate(params object[] objects)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        foreach (var item in objects)
        {
            var itemType = item.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(itemType);
            var validator = scope.ServiceProvider.GetRequiredService(validatorType);
            var validationResult = (ValidationResult)validator.GetType().GetMethod(nameof(IValidator.Validate), [itemType])!.Invoke(validator, new[] { item })!;
            if (!validationResult.IsValid)
            {
                return Response.Failure(validationResult.Errors.Select(e => new Error(e.ErrorCode, e.ErrorMessage)));
            }
        }

        return Response.Success();
    }
}
