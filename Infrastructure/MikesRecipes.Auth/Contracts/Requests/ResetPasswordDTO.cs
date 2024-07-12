namespace MikesRecipes.Auth.Contracts.Requests;

public record ResetPasswordDTO(string Email, string DecodedToken, string NewPassword);
