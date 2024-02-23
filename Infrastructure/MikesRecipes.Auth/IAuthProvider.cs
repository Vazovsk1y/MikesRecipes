using MikesRecipes.Auth.Contracts;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IAuthProvider
{
	Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default);

	Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default);

	Task<Response<string>> RefreshTokenAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default);
}