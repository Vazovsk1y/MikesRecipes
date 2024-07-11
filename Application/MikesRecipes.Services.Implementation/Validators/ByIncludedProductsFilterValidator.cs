using FluentValidation;
using MikesRecipes.Application.Contracts;
using MikesRecipes.Application.Contracts.Requests;

namespace MikesRecipes.Services.Implementation.Validators;

public class ByIncludedProductsFilterValidator : AbstractValidator<ByIncludedProductsFilter>
{
    public ByIncludedProductsFilterValidator()
    {
        RuleFor(e => e.IncludedProducts).NotEmpty()
       .Must(e =>
        {
            return e.Count() == e.Distinct().Count();
        })
       .WithMessage("Included products contain duplicates.");

        RuleFor(e => e.OtherProductsCount).GreaterThanOrEqualTo(0);
    }
}