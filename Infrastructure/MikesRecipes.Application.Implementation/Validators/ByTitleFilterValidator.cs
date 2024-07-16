using FluentValidation;
using MikesRecipes.Application.Contracts.Requests;

namespace MikesRecipes.Application.Implementation.Validators;

public class ByTitleFilterValidator : AbstractValidator<ByTitleFilter>
{
    public ByTitleFilterValidator()
    {
        RuleFor(e => e.Value).NotEmpty();
    }
}