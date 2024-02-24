using MikesRecipes.Framework.Contracts;

namespace MikesRecipes.Framework.Interfaces;

public interface ICurrentUserProvider
{
    bool IsAuthenticated { get; }
    CurrentUser? Get();
}