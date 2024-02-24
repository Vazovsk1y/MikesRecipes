using System.Security.Claims;

namespace MikesRecipes.Framework.Contracts;

 public record CurrentUser(Guid Id, IEnumerable<Claim> Claims);
