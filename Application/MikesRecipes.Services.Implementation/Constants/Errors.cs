using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Services.Implementation.Constants;

public static class Errors
{
    public const string ValidationErrorCode = "ValidationError";

    public const string AccessDeniedErrorCode = "AccessDenied";

    public static Error NullOrWhiteSpaceString(string propertyName) => new(ValidationErrorCode, $"{propertyName} was equal to null or whitespace.");

    public static readonly Error Unauthorized = new(AccessDeniedErrorCode, "Unauthorized.");
}
