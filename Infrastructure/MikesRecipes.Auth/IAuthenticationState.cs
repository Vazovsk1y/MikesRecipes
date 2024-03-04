using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IAuthenticationState
{
	Task<Response<User>> IsAuthenticatedAsync(bool checkEmail = true, bool checkSecurityStamp = true, CancellationToken cancellationToken = default);
}