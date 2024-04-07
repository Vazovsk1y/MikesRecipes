using MikesRecipes.Domain.Shared;

namespace MikesRecipes.Framework.Constants;

public static class Errors
{
    public static readonly Error EmailNotSent = new("Email", "Email was not sent. Something went wrong.");
}
