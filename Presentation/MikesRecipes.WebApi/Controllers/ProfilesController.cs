using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.Auth;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Filters;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(ApiVersions.V1Dot0)]
[Authorize]
[ValidateSecurityStampFilter]
[ConfirmedEmailFilter]
public class ProfilesController(IUserProfileService userProfileService) : BaseController
{
    private readonly IUserProfileService _userProfileService = userProfileService;

    [HttpPatch("change-email")]
    public async Task<IActionResult> ChangeEmail([Required][EmailAddress]string newEmail, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.ChangeEmailAsync(newEmail, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }
}