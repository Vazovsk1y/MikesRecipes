using FluentValidation;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.Auth.Implementation.Validators;

public class ResetPasswordDTOValidator : AbstractValidator<ResetPasswordDTO>
{
    public ResetPasswordDTOValidator()
    {
        RuleFor(e => e.NewPassword).NotEmpty();
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
        RuleFor(e => e.DecodedToken).NotEmpty();
    }
}