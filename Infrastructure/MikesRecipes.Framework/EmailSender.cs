using FluentEmail.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework.Constants;
using MikesRecipes.Framework.Contracts;
using MikesRecipes.Framework.Interfaces;

namespace MikesRecipes.Framework;

public class EmailSender(
    IClock clock,
    ILogger<BaseService> logger,
    IServiceScopeFactory serviceScopeFactory,
    IFluentEmail fluentEmail)
    : BaseService(clock, logger, serviceScopeFactory), IEmailSender
{
    public async Task<Response> SendEmailAsync(EmailDTO emailDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(emailDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure(validationResult.Errors);
        }

        var sendResponse = await fluentEmail
            .To(emailDTO.To, emailDTO.ToName)
            .Subject(emailDTO.Subject)
            .Body(emailDTO.Body, emailDTO.IsHtml)
            .Tag(emailDTO.Purpose)
            .SendAsync(cancellationToken);

        if (sendResponse.Successful)
        {
            _logger.LogInformation("Email with purpose '{emailPurpose}' was successfully sent to '{emailReceiver}'.", emailDTO.Purpose, emailDTO.To);
            return Response.Success();
        }

        _logger.LogError("Email with purpose '{emailPurpose}' was not sent to '{emailReceiver}'.\nErrors:\n{@errorMessages}", emailDTO.Purpose, emailDTO.To, sendResponse.ErrorMessages);
        return Response.Failure(Errors.EmailNotSent);
    }
}