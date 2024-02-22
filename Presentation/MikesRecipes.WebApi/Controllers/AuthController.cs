using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.Services;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(Constants.ApiVersions.V1Dot0)]
public class AuthController(IAuthProvider authProvider) : BaseController
{
    private readonly IAuthProvider _authProvider = authProvider;

    [HttpPost("sign-up")]
    public async Task<IActionResult> RegisterAsync(UserRegisterModel registerModel, CancellationToken cancellationToken)
    {
        var dto = registerModel.ToDTO();
        var result = await _authProvider.RegisterAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> LoginAsync(UserLoginModel loginModel, CancellationToken cancellationToken)
    {
        var dto = loginModel.ToDTO();
        var result = await _authProvider.LoginAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(RefreshModel refreshModel, CancellationToken cancellationToken)
    {
        var dto = refreshModel.ToDTO();
        var result = await _authProvider.RefreshTokenAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}