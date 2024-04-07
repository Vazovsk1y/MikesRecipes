namespace MikesRecipes.Auth.Contracts;

public record ResetPasswordDTO(string Email, string DecodedToken, string NewPassword);
