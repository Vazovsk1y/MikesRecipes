using MikesRecipes.Domain.Shared;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services;

public interface IAuthProvider
{
	Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default);

	Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default);

	Task<Response<string>> RefreshTokenAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default);
}