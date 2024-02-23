using FluentValidation;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementation.Validators;

public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
{
    public UserLoginDTOValidator()
    {
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
        RuleFor(e => e.Password).NotEmpty();
    }
}