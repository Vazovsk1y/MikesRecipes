using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Services.Implementations;

public static class Errors
{
    public const string ValidationErrorCode = "Validation";

    public static Error NullOrWhiteSpaceString(string propertyName) => new(ValidationErrorCode, $"{propertyName} was equal to null or whitespace.");
    public static class Auth
    {
        private const string CodeBase = "Auth";

        public static readonly Error InvalidEmailOrPassword = new($"{CodeBase}.{nameof(InvalidEmailOrPassword)}", "Invalid email or password.");

        public static readonly Error UserLockedOut = new($"{CodeBase}.{nameof(UserLockedOut)}", "You have reached the maximum number of login attempts try again later.");

        public static readonly Error EmailAlreadyTaken = new($"{CodeBase}.{nameof(EmailAlreadyTaken)}", "Email is already taken.");

        public static readonly Error InvalidAccessToken = new($"{CodeBase}.{nameof(InvalidAccessToken)}", "Invalid access token.");

        public static readonly Error InvalidRefreshToken = new($"{CodeBase}.{nameof(InvalidRefreshToken)}", "Refresh token was not found or invalid.");

        public static readonly Error RefreshTokenExpired = new($"{CodeBase}.{nameof(RefreshTokenExpired)}", "Refresh token has expired.");
    }
}
