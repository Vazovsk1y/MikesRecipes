namespace MikesRecipes.Auth.Contracts.Requests;

public record EmailConfirmationDTO(Guid UserId, string DecodedToken);
