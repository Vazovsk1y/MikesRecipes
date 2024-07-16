using FluentValidation;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.Auth.Implementation.Validators;

public class EmailConfirmationDTOValidator : AbstractValidator<EmailConfirmationDTO>
{
    public EmailConfirmationDTOValidator()
    {
        RuleFor(e => e.UserId).NotEmpty();
        RuleFor(e => e.DecodedToken).NotEmpty();
    }
}