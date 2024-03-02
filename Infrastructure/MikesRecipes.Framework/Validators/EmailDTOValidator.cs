using FluentValidation;
using MikesRecipes.Framework.Contracts;

namespace MikesRecipes.Framework.Validators;

public class EmailDTOValidator : AbstractValidator<EmailDTO>
{
    public EmailDTOValidator()
    {
        RuleFor(e => e.To).NotEmpty().EmailAddress();
        RuleFor(e => e.Subject).NotEmpty();
        RuleFor(e => e.Body).NotEmpty();
        When(e => e.ToName is not null, () =>
        {
            RuleFor(e => e.ToName).NotEmpty().NotEqual(e => e.To);
        });
        RuleFor(e => e.Purpose).NotEmpty();
    }
}