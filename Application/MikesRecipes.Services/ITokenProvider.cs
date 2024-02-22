using System.Security.Claims;

namespace MikesRecipes.Services;

public interface ITokenProvider
{
	string GenerateAccessToken(ClaimsPrincipal claimsPrincipal);

	string GenerateRefreshToken();
}