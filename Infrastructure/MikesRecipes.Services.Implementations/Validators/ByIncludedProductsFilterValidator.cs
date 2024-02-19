using FluentValidation;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services.Implementations.Validators;

public class ByIncludedProductsFilterValidator : AbstractValidator<ByIncludedProductsFilter>
{
    public ByIncludedProductsFilterValidator()
    {
        RuleFor(e => e.ProductIds).NotEmpty()
       .Must(e =>
        {
            return e.Count() == e.Distinct().Count();
        })
       .WithMessage("Included products contain duplicates.");

        RuleFor(e => e.OtherProductsCount).GreaterThanOrEqualTo(0);
    }
}
