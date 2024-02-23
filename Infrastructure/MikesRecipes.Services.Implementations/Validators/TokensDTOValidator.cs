using FluentValidation;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations.Validators;

public class TokensDTOValidator : AbstractValidator<TokensDTO>
{
    public TokensDTOValidator()
    {
        RuleFor(e => e.RefreshToken).NotEmpty();
        RuleFor(e => e.AccessToken).NotEmpty();
    }
}