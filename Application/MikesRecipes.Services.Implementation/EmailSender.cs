using FluentEmail.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Implementation.Constants;

namespace MikesRecipes.Services.Implementation;

public class EmailSender : BaseService, IEmailSender
{
    private readonly IFluentEmail _fluentEmail;

    public EmailSender(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IFluentEmail fluentEmail) : base(clock, logger, serviceScopeFactory)
    {
        _fluentEmail = fluentEmail;
    }

    public async Task<Response> SendEmailAsync(EmailDTO emailDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(emailDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure(validationResult.Errors);
        }

        var sendResponse = await _fluentEmail
            .To(emailDTO.To, emailDTO.ToName)
            .Subject(emailDTO.Subject)
            .Body(emailDTO.Body, emailDTO.IsHtml)
            .Tag(emailDTO.Purpose)
            .SendAsync(cancellationToken);

        if (sendResponse.Successful)
        {
            _logger.LogInformation("Email with purpose '{emailPurpose}' was successfully sent to '{receiver}'.", emailDTO.Purpose, emailDTO.To);
            return Response.Success();
        }
        else
        {
            _logger.LogError("Email with purpose '{emailPurpose}' was not sent to '{receiver}'.\nErrors:\n{errorMessages}", emailDTO.Purpose, emailDTO.To, sendResponse.ErrorMessages);
            return Response.Failure(Errors.EmailNotSent);
        }
    }
}