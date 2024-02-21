namespace MikesRecipes.Services.Contracts;

public static class Auth
{
    public record UserRegisterDTO(string Username, string Email, string Password);

    public record UserLoginDTO(string Email, string Password);

    public record TokensDTO(string AccessToken, string RefreshToken);

    public record GenerateAccessTokenDTO(Guid UserId, string Username, string Email, IEnumerable<string> Roles);
}