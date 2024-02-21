using FluentValidation;
using Microsoft.AspNetCore.Identity;
using MikesRecipes.Domain.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations;

public class AuthProvider(
    IValidator<UserRegisterDTO> userRegisterValidator,
    UserManager<User> userManager) : IAuthProvider
{
    private readonly IValidator<UserRegisterDTO> _userRegisterValidator = userRegisterValidator;
    private readonly UserManager<User> _userManager = userManager;

    public Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Response<TokensDTO>> RefreshToken(TokensDTO tokensDTO, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default)
    {
        var validationResult = _userRegisterValidator.Validate(userRegisterDTO);
        if (!validationResult.IsValid)
        {
            return Response.Failure(new Error(validationResult.ToString()));
        }

        var userWithPassedEmail = await _userManager.FindByEmailAsync(userRegisterDTO.Email);
        if (userWithPassedEmail is not null)
        {
            return Response.Failure(new Error("Email is already taken."));
        }

        var user = new User
        {
            Email = userRegisterDTO.Email.Trim(),
            UserName = userRegisterDTO.Username.Trim(),
        };

        var creationResult = await _userManager.CreateAsync(user, userRegisterDTO.Password);
        if (!creationResult.Succeeded)
        {
            return Response.Failure(new Error(string.Join(Environment.NewLine, creationResult.Errors.Select(e => e.Description))));
        }

        var addedUser = await _userManager.FindByEmailAsync(user.Email);
        await _userManager.AddToRoleAsync(user, DefaultRoles.User);
        return Response.Success();
    }
}
