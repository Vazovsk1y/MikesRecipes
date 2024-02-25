using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth.Implementation.Constants;

public static class Errors
{
    private const string CodeBase = "Auth";

    public static readonly Error InvalidEmailOrPassword = new($"{CodeBase}.{nameof(InvalidEmailOrPassword)}", "Invalid email or password.");

    public static readonly Error UserLockedOut = new($"{CodeBase}.{nameof(UserLockedOut)}", "You have reached the maximum number of login attempts try again later.");

    public static readonly Error InvalidAccessToken = new($"{CodeBase}.{nameof(InvalidAccessToken)}", "Invalid access token.");

    public static readonly Error InvalidRefreshToken = new($"{CodeBase}.{nameof(InvalidRefreshToken)}", "Refresh token was not found or invalid.");

    public static readonly Error RefreshTokenExpired = new($"{CodeBase}.{nameof(RefreshTokenExpired)}", "Refresh token has expired.");

    public static readonly Error RegistrationFailed = new($"{CodeBase}.{nameof(RegistrationFailed)}", "Registration failed.");

    public static Error ConfirmationRequiredFor(string confirmationFor) => new($"{CodeBase}.ConfirmationRequired", $"{confirmationFor} is not confirmed.");
}
