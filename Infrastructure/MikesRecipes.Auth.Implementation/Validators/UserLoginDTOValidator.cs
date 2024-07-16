using FluentValidation;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.Auth.Implementation.Validators;

public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
{
    public UserLoginDTOValidator()
    {
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
        RuleFor(e => e.Password).NotEmpty();
    }
}