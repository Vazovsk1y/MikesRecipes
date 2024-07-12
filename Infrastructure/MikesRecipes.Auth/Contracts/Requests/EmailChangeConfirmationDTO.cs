namespace MikesRecipes.Auth.Contracts.Requests;

public record EmailChangeConfirmationDTO(Guid UserId, string DecodedToken, string NewEmail) : EmailConfirmationDTO(UserId, DecodedToken);
