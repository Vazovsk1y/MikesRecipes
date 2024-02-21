using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services;

public interface ITokenProvider
{
	string GenerateAccessToken(GenerateAccessTokenDTO generateAccessTokenDTO);

	string GenerateRefreshToken();
}