using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MikesRecipes.Auth;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.Filters;
using MikesRecipes.WebApi.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MikesRecipes.Auth.Contracts.Requests;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(ApiVersions.V1Dot0)]
public class AuthController(IAuthenticationService authenticationService) : BaseController
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    [HttpPost("sign-up")]
    public async Task<IActionResult> Register(UserRegisterModel registerModel, CancellationToken cancellationToken)
    {
        var dto = registerModel.ToDTO();
        var result = await _authenticationService.RegisterAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> Login(UserLoginModel loginModel, CancellationToken cancellationToken)
    {
        var dto = loginModel.ToDTO();
        var result = await _authenticationService.LoginAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [Authorize]
    [ValidateSecurityStampFilter]
    [ConfirmedEmailFilter]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokens([Required]string refreshToken, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokensAsync(refreshToken, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token, [EmailAddress]string? newEmail, CancellationToken cancellationToken)
    {
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return BadRequest();
        }

        var result = string.IsNullOrWhiteSpace(newEmail) ?
            await _authenticationService.ConfirmEmailAsync(new EmailConfirmationDTO(userId, token), cancellationToken)
            :
            await _authenticationService.ConfirmEmailChangeAsync(new EmailChangeConfirmationDTO(userId, token, newEmail), cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [Authorize]
    [ValidateSecurityStampFilter]
    [ConfirmedEmailFilter]
    [HttpDelete("revoke")]
    public async Task<IActionResult> RevokeRefreshToken(CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RevokeRefreshTokenAsync(cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([Required][EmailAddress]string email, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ResendEmailConfirmationAsync(email, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}