using FluentValidation;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.Auth.Implementation.Validators;

public class EmailChangeConfirmationDTOValidator : AbstractValidator<EmailChangeConfirmationDTO>
{
    public EmailChangeConfirmationDTOValidator()
    {
        RuleFor(e => e.UserId).NotEmpty();
        RuleFor(e => e.DecodedToken).NotEmpty();
        RuleFor(e => e.NewEmail).NotEmpty().EmailAddress();
    }
}