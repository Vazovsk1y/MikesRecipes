using MikesRecipes.Auth.Contracts;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IExternalAuthenticationService
{
	Task<Response<TokensDTO>> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default);
}