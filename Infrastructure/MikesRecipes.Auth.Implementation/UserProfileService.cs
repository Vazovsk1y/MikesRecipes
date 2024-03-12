using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Auth.Implementation.Extensions;
using MikesRecipes.Auth.Implementation.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation;

public class UserProfileService : BaseAuthService, IUserProfileService
{
    private readonly IEmailConfirmationsSender _emailConfirmationsSender;
    private readonly IAuthenticationState _authenticationState;
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    private readonly AuthSignInOptions _signInOptions;

    public UserProfileService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentUserProvider currentUserProvider,
        UserManager<User> userManager,
        IEmailConfirmationsSender emailConfirmationsSender,
        IOptions<AuthOptions> authOptions,
        MikesRecipesDbContext dbContext,
        SignInManager<User> signInManager,
        IAuthenticationState authenticationState) : base(clock, logger, serviceScopeFactory, currentUserProvider, userManager, authOptions, dbContext, signInManager)
    {
        _authenticationState = authenticationState;
        _emailConfirmationsSender = emailConfirmationsSender;
        _signInOptions = _authOptions.SignIn;
    }

    public async Task<Response> ChangeEmailAsync(string newEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        if (isAuthenticatedResponse.IsFailure)
        {
            return isAuthenticatedResponse;
        }

        User user = isAuthenticatedResponse.Value;
        Response response;

        if (_signInOptions.RequireConfirmedEmail)
        {
            newEmail = newEmail.Trim();
            if (string.IsNullOrWhiteSpace(newEmail)
                || !EmailAddressAttribute.IsValid(newEmail)
                || user.Email == newEmail
                || await _userManager.FindByEmailAsync(newEmail) is not null)
            {
                return Response.Failure(Errors.InvalidEmailOrPassword);
            }

            response = await _emailConfirmationsSender.SendEmailChangeConfirmationLinkAsync(user, newEmail, cancellationToken);
        }
        else
        {
            var changeEmailResult = await _userManager.SetEmailAsync(user, newEmail);
            response = changeEmailResult.Succeeded ? Response.Success() : Response.Failure(changeEmailResult.Errors.Select(e => new Error($"{Errors.CodeBase}.{e.Code}", e.Description)));
        }

        return response;
    }

    public async Task<Response> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email) 
            || !EmailAddressAttribute.IsValid(email) 
            || await _userManager.FindByEmailAsync(email) is not User user)
        {
            return Response.Failure(Errors.InvalidEmailOrPassword);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Response.Failure(Errors.ConfirmationRequiredFor(nameof(User.Email)));
        }

        var result = await _emailConfirmationsSender.SendResetPasswordConfirmationLinkAsync(user, cancellationToken);
        return result;
    }

    public async Task<Response> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(resetPasswordDTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);

        switch (user)
        {
            case null:
                return Response.Failure(Errors.InvalidEmailOrPassword);
            case User when !await _userManager.IsEmailConfirmedAsync(user):
                return Response.Failure(Errors.ConfirmationRequiredFor(nameof(User.Email)));
            default:
                {
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.DecodedToken, resetPasswordDTO.NewPassword);
                    return result.Succeeded ? Response.Success() : Response.Failure(result.Errors.ToErrors());
                }
        }
    }

    public async Task<Response<UserProfileDTO>> GetProfileInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var authResult = await _authenticationState.IsAuthenticatedAsync(cancellationToken: cancellationToken);
        return authResult.IsFailure ? Response.Failure<UserProfileDTO>(authResult.Errors) : authResult.Value.ToDTO();
    }
}