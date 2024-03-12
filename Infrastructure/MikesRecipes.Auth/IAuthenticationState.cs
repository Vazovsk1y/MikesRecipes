using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IAuthenticationState
{
	Task<Response<User>> IsAuthenticatedAsync(bool validateConfirmedEmail = true, bool validateSecurityStamp = true, CancellationToken cancellationToken = default);
}