using FluentValidation;
using MikesRecipes.Application.Contracts.Common;

namespace MikesRecipes.Services.Implementation.Validators;

public class PagingOptionsValidator : AbstractValidator<PagingOptions>
{
    public PagingOptionsValidator() 
    {
        RuleFor(e => e.PageIndex).GreaterThanOrEqualTo(1);
        RuleFor(e => e.PageSize).GreaterThanOrEqualTo(1);
    }
}