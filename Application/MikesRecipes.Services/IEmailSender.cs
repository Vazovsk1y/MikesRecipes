using MikesRecipes.Domain.Shared;
using MikesRecipes.Services.Contracts;

namespace MikesRecipes.Services;

public interface IEmailSender
{
	Task<Response> SendEmailAsync(EmailDTO emailDTO, CancellationToken cancellationToken = default);
}
