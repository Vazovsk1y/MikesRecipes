using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework.Contracts;

namespace MikesRecipes.Framework.Interfaces;

public interface IEmailSender
{
    Task<Response> SendEmailAsync(EmailDTO emailDTO, CancellationToken cancellationToken = default);
}
