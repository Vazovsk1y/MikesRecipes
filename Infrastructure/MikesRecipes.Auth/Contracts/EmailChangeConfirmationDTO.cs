namespace MikesRecipes.Auth.Contracts;

public record EmailChangeConfirmationDTO(Guid UserId, string DecodedToken, string NewEmail) : EmailConfirmationDTO(UserId, DecodedToken);
