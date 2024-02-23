using FluentValidation;
using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Services.Implementation.Validators;

public class PagingOptionsValidator : AbstractValidator<PagingOptions>
{
    public PagingOptionsValidator() 
    {
        RuleFor(e => e.PageIndex).GreaterThanOrEqualTo(1);
        RuleFor(e => e.PageSize).GreaterThanOrEqualTo(1);
    }
}