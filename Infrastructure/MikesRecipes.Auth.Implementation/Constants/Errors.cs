using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Auth.Implementation.Constants;

public static class Errors
{
    public const string BaseCode = "Auth";

    public static readonly Error InvalidEmailOrPassword = new($"{BaseCode}.{nameof(InvalidEmailOrPassword)}", "Invalid email or password.");

    public static readonly Error UserLockedOut = new($"{BaseCode}.{nameof(UserLockedOut)}", "You have reached the maximum number of login attempts try again later.");

    public static readonly Error InvalidRefreshToken = new($"{BaseCode}.{nameof(InvalidRefreshToken)}", "Refresh token was not found or invalid.");

    public static readonly Error RegistrationFailed = new($"{BaseCode}.{nameof(RegistrationFailed)}", "Registration failed.");

    public static readonly Error UserNotFound = new($"{BaseCode}.{nameof(UserNotFound)}", "User not found.");

    public static readonly Error EmailIsAlreadyConfirmed = new($"{BaseCode}.{nameof(EmailIsAlreadyConfirmed)}", "Email is already confirmed.");

    public static readonly Error Unauthorized = new($"{BaseCode}.{nameof(Unauthorized)}", "Unauthorized.");

    public static readonly Error EmailConfirmationDisabled = new($"{BaseCode}.{nameof(EmailConfirmationDisabled)}", "Email confirmation disabled.");
    public static Error ConfirmationRequiredFor(string confirmationFor) => new($"{BaseCode}.ConfirmationRequired", $"{confirmationFor} is not confirmed.");
}
