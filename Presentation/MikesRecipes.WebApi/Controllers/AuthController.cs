﻿using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MikesRecipes.Auth;
using MikesRecipes.Auth.Contracts;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;
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
        string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var dto = new EmailConfirmationDTO(userId, decodedToken);
        var result = await _authProvider.ConfirmEmailAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}