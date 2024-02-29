using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IEmailConfirmationsSender
{
	Task<Response> SendEmailConfirmationLinkAsync(User user, CancellationToken cancellationToken = default);
}