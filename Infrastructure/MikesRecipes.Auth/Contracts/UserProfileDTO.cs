namespace MikesRecipes.Auth.Contracts;

public record UserProfileDTO(
    string Email, 
    bool IsEmailConfirmed, 
    string Username);
