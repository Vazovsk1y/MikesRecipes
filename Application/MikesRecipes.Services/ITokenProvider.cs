using MikesRecipes.Domain.Shared;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services;

public interface ITokenProvider
{
	Response<string> GenerateAccessToken(GenerateAccessTokenDTO generateAccessTokenDTO);

	Response<string> GenerateRefreshToken();
}