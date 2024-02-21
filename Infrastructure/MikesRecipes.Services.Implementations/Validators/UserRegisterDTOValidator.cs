using FluentValidation;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations.Validators;

public class UserRegisterDTOValidator : AbstractValidator<UserRegisterDTO>
{
    public UserRegisterDTOValidator()
    {
        RuleFor(e => e.Username).NotEmpty();
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
        RuleFor(e => e.Password).NotEmpty();
    }
}