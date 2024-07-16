using FluentValidation;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.Auth.Implementation.Validators;

public class UserRegisterDTOValidator : AbstractValidator<UserRegisterDTO>
{
    public UserRegisterDTOValidator()
    {
        RuleFor(e => e.Username).NotEmpty();
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
        RuleFor(e => e.Password).NotEmpty();
    }
}