using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IUserProfileService
{
	Task<Response> ChangeEmailAsync(string newEmail, CancellationToken cancellationToken = default);
}