using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MikesRecipes.Auth;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(ApiVersions.V1Dot0)]
public class AuthController(IAuthProvider authProvider) : BaseController
{
    private readonly IAuthProvider _authProvider = authProvider;

    [HttpPost("sign-up")]
    public async Task<IActionResult> Register(UserRegisterModel registerModel, CancellationToken cancellationToken)
    {
        var dto = registerModel.ToDTO();
        var result = await _authProvider.RegisterAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> Login(UserLoginModel loginModel, CancellationToken cancellationToken)
    {
        var dto = loginModel.ToDTO();
        var result = await _authProvider.LoginAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAccessToken(RefreshModel refreshModel, CancellationToken cancellationToken)
    {
        var dto = refreshModel.ToDTO();
        var result = await _authProvider.RefreshAccessTokenAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token, CancellationToken cancellationToken)
    {
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return BadRequest();
        }

        var dto = new EmailConfirmationDTO(userId, token);
        var result = await _authProvider.ConfirmEmailAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [Authorize]
    [HttpDelete("revoke")]
    public async Task<IActionResult> RevokeRefreshToken(CancellationToken cancellationToken)
    {
        var result = await _authProvider.RevokeRefreshTokenAsync(cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([Required][EmailAddress]string email, CancellationToken cancellationToken)
    {
        var result = await _authProvider.ResendEmailConfirmationAsync(email, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}