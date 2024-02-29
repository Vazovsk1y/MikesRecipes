using MikesRecipes.Auth.Contracts;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IAuthProvider
{
	Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default);

	Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default);

	Task<Response<string>> RefreshAccessTokenAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default);

	Task<Response> ConfirmEmailAsync(EmailConfirmationDTO emailConfirmationDTO, CancellationToken cancellationToken = default);

	Task<Response> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default);
}