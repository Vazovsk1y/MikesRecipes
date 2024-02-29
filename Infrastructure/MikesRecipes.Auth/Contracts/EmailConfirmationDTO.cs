namespace MikesRecipes.Auth.Contracts;

public record EmailConfirmationDTO(Guid UserId, string DecodedToken);
