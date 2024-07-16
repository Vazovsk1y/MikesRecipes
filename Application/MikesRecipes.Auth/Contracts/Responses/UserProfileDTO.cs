namespace MikesRecipes.Auth.Contracts.Responses;

public record UserProfileDTO(
    string Email, 
    bool IsEmailConfirmed, 
    string Username);
