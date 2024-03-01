using Microsoft.AspNetCore.Identity;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Auth.Implementation;

public class UserProfileService : IUserProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailConfirmationsSender _emailConfirmationsSender;
    private readonly ICurrentUserProvider _currentUserProvider;
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    public UserProfileService(
        UserManager<User> userManager, 
        IEmailConfirmationsSender emailConfirmationsSender, 
        ICurrentUserProvider currentUserProvider)
    {
        _userManager = userManager;
        _emailConfirmationsSender = emailConfirmationsSender;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Response> ChangeEmailAsync(string newEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentUser = _currentUserProvider.Get();
        User? user = _currentUserProvider.IsAuthenticated && currentUser is not null ?
            await _userManager.FindByIdAsync(currentUser.Id.ToString())
            :
            null;

        if (user is null)
        {
            return Response.Failure(Errors.Unauthorized);
        }

        newEmail = newEmail.Trim();
        if (string.IsNullOrWhiteSpace(newEmail) 
            || !EmailAddressAttribute.IsValid(newEmail) 
            || user.Email == newEmail 
            || await _userManager.FindByEmailAsync(newEmail) is not null)
        {
            return Response.Failure(Errors.InvalidEmailOrPassword);
        }

        var response = await _emailConfirmationsSender.SendEmailChangeConfirmationLinkAsync(user, newEmail, cancellationToken);
        return response;
    }
}