using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Services.Implementation.Constants;

public static class Errors
{
    public const string ValidationErrorCode = "Validation";

    public static Error NullOrWhiteSpaceString(string propertyName) => new(ValidationErrorCode, $"{propertyName} was equal to null or whitespace.");
}
