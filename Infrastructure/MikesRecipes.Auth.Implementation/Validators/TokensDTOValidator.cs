using FluentValidation;
using MikesRecipes.Auth.Contracts;

namespace MikesRecipes.Auth.Implementation.Validators;

public class TokensDTOValidator : AbstractValidator<TokensDTO>
{
    public TokensDTOValidator()
    {
        RuleFor(e => e.RefreshToken).NotEmpty();
        RuleFor(e => e.AccessToken).NotEmpty();
    }
}