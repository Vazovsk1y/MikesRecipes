using MikesRecipes.Auth.Contracts;
using MikesRecipes.Auth.Contracts.Requests;
using MikesRecipes.Auth.Contracts.Responses;
using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth;

public interface IUserProfileService
{
	Task<Response> ChangeEmailAsync(string newEmail, CancellationToken cancellationToken = default);

	Task<Response> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

	Task<Response> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO, CancellationToken cancellationToken = default);

	Task<Response<UserProfileDTO>> GetProfileInfoAsync(CancellationToken cancellationToken = default);
}