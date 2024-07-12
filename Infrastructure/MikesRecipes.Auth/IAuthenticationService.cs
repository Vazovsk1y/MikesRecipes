using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Contracts.Requests;
using MikesRecipes.Auth.Contracts.Responses;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IAuthenticationService
{
	Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default);

	Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default);

	Task<Response<TokensDTO>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);

	Task<Response> ConfirmEmailAsync(EmailConfirmationDTO emailConfirmationDTO, CancellationToken cancellationToken = default);

	Task<Response> ConfirmEmailChangeAsync(EmailChangeConfirmationDTO emailChangeConfirmationDTO, CancellationToken cancellationToken = default);

	Task<Response> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default);

	Task<Response> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default);
}