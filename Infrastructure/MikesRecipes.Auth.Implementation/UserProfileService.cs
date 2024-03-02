using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikesRecipes.Auth.Implementation.Constants;
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
        MikesRecipesDbContext dbContext) : base(clock, logger, serviceScopeFactory, currentUserProvider, userManager, authOptions, dbContext)
    {
        _emailConfirmationsSender = emailConfirmationsSender;
        _signInOptions = _authOptions.SignIn;
    }

    public async Task<Response> ChangeEmailAsync(string newEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAuthenticatedResponse = await IsAuthenticated();
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
}