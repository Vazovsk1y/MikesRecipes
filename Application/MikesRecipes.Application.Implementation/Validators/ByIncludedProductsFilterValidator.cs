using FluentValidation;
using MikesRecipes.Application.Contracts.Requests;

namespace MikesRecipes.Application.Implementation.Validators;

public class ByIncludedProductsFilterValidator : AbstractValidator<ByIncludedProductsFilter>
{
    public ByIncludedProductsFilterValidator()
    {
        RuleFor(e => e.IncludedProducts).NotEmpty();
        RuleFor(e => e.OtherProductsCount).GreaterThanOrEqualTo(0);
    }
}