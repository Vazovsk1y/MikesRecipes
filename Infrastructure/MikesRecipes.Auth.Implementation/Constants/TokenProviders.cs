namespace MikesRecipes.Auth.Implementation.Constants;

public static class TokenProviders
{
    public static class RefreshTokenProvider
    {
        public const string Name = "Refresh_token";

        public const string LoginProvider = "MikesRecipes";
    }

    public static class AccessTokenProvider
    {
        public const string Name = "Access_token";

        public const string LoginProvider = "Jwt";
    }
}
