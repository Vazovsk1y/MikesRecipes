using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using MikesRecipes.Framework;
using MikesRecipes.Framework.Interfaces;
using MikesRecipes.Services.Contracts;
using System.Text;
using System.Text.Encodings.Web;

namespace MikesRecipes.Auth.Implementation;

public class EmailConfirmationsSender : BaseService, IEmailConfirmationsSender
{


    private readonly UserManager<User> _userManager;
    private readonly HttpContext _httpContext;
    private readonly IUrlHelper _urlHelper;
    private readonly Services.IEmailSender _emailSender;
    public EmailConfirmationsSender(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        Services.IEmailSender emailSender) : base(clock, logger, serviceScopeFactory)
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
}
