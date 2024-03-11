using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Auth.Implementation.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Contracts;
using MikesRecipes.Framework.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace MikesRecipes.Auth.Implementation;

public class EmailConfirmationsSender : BaseService, IEmailConfirmationsSender
{
    private readonly UserManager<User> _userManager;
    private readonly HttpContext _httpContext;
    private readonly IUrlHelper _urlHelper;
    private readonly IEmailSender _emailSender;
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    public EmailConfirmationsSender(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IEmailSender emailSender) : base(clock, logger, serviceScopeFactory)
    {
        _userManager = userManager;

        var httpContext = httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        _httpContext = httpContext;

        var actionContext = actionContextAccessor.ActionContext;
        ArgumentNullException.ThrowIfNull(actionContext, nameof(actionContext));

        _urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
        _emailSender = emailSender;
    }

    public async Task<Response> SendEmailConfirmationLinkAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        const string ActionTitle = "ConfirmEmail";
        const string ControllerTitle = "Auth";

        var urlContext = new UrlActionContext
        {
            Action = ActionTitle,
            Controller = ControllerTitle,
            Host = _httpContext.Request.Host.Value,
            Protocol = _httpContext.Request.Scheme,
            Values = new { userId = user.Id, token }
        };

        string? confirmationLink = _urlHelper.Action(urlContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationLink, nameof(confirmationLink));

        const string LetterSubject = "Email confirmation";
        const string LetterPurpose = "User email confirmation";
        string body = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.";
        var letter = new EmailDTO(user.Email!, LetterSubject, body)
        {
            ToName = user.UserName,
            Purpose = LetterPurpose
        };

        var result = await _emailSender.SendEmailAsync(letter, cancellationToken);
        return result;
    }

    public async Task<Response> SendEmailChangeConfirmationLinkAsync(User user, string newEmail, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        const string ActionTitle = "ConfirmEmail";
        const string ControllerTitle = "Auth";

        if (string.IsNullOrWhiteSpace(newEmail) || !EmailAddressAttribute.IsValid(newEmail))
        {
            return Response.Failure(Errors.InvalidEmailOrPassword);
        }

        string token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var urlContext = new UrlActionContext
        {
            Action = ActionTitle,
            Controller = ControllerTitle,
            Host = _httpContext.Request.Host.Value,
            Protocol = _httpContext.Request.Scheme,
            Values = new { userId = user.Id, token, newEmail }
        };

        string? confirmationLink = _urlHelper.Action(urlContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationLink, nameof(confirmationLink));

        const string LetterSubject = "Change email";
        const string LetterPurpose = "User change email confirmation";
        string body = $"You try to change your current email '{user.Email}' to '{newEmail}'. Please confirm your action by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.";
        var letter = new EmailDTO(newEmail, LetterSubject, body)
        {
            ToName = user.UserName,
            Purpose = LetterPurpose
        };

        var result = await _emailSender.SendEmailAsync(letter, cancellationToken);
        return result;
    }

    public async Task<Response> SendResetPasswordConfirmationLinkAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string ActionTitle = "ResetPassword";
        const string ControllerTitle = "Profiles";

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var urlContext = new UrlActionContext
        {
            Action = ActionTitle,
            Controller = ControllerTitle,
            Host = _httpContext.Request.Host.Value,
            Protocol = _httpContext.Request.Scheme,
            Values = new { email = user.Email, token }
        };

        string? confirmationLink = _urlHelper.Action(urlContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationLink, nameof(confirmationLink));

        const string LetterSubject = "Reset password";
        const string LetterPurpose = "User reset password letter";
        string body = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.";
        var letter = new EmailDTO(user.Email!, LetterSubject, body)
        {
            ToName = user.UserName,
            Purpose = LetterPurpose
        };

        var result = await _emailSender.SendEmailAsync(letter, cancellationToken);
        return result;
    }
}
