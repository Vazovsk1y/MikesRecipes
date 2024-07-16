using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.Auth;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;
using System.ComponentModel.DataAnnotations;
using MikesRecipes.WebApi.Infrastructure.Filters;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(Constants.WebApi.Version)]
public class ProfilesController(IUserProfileService userProfileService) : BaseController
{
    [HttpPatch("change-email")]
    [Authorize, ValidateSecurityStampFilter, ConfirmedEmailFilter]
    public async Task<IActionResult> ChangeEmail([Required][EmailAddress] string newEmail, CancellationToken cancellationToken)
    {
        var result = await userProfileService.ChangeEmailAsync(newEmail, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([Required][EmailAddress] string email, CancellationToken cancellationToken)
    {
        var result = await userProfileService.ForgotPasswordAsync(email, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword([Required][EmailAddress] string email, [Required] string token)
    {
        var resetPasswordPage = $$"""
                                     
                                                 <!DOCTYPE html>
                                         <html lang="en">
                                         <head>
                                             <meta charset="UTF-8">
                                             <meta name="viewport" content="width=device-width, initial-scale=1.0">
                                             <title>Reset password</title>
                                             <style>
                                                 body {
                                                     font-family: Arial, sans-serif;
                                                     background-color: #f4f4f4;
                                                     margin: 0;
                                                     padding: 0;
                                                     display: flex;
                                                     justify-content: center;
                                                     align-items: center;
                                                     height: 100vh;
                                                 }
                                                 .password-reset-form {
                                                     background-color: #fff;
                                                     padding: 20px;
                                                     border-radius: 8px;
                                                     box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                                     max-width: 400px;
                                                     width: 100%;
                                                 }
                                                 .form-group {
                                                     margin-bottom: 20px;
                                                 }
                                                 label {
                                                     font-weight: bold;
                                                 }
                                                 input[type="email"],
                                                 input[type="password"] {
                                                     width: 100%;
                                                     padding: 10px;
                                                     border: 1px solid #ccc;
                                                     border-radius: 4px;
                                                 }
                                                 button[type="submit"] {
                                                     background-color: #007bff;
                                                     color: #fff;
                                                     padding: 10px 20px;
                                                     border: none;
                                                     border-radius: 4px;
                                                     cursor: pointer;
                                                 }
                                                 button[type="submit"]:hover {
                                                     background-color: #0056b3;
                                                 }
                                             </style>
                                         </head>
                                         <body>
                                             <div class="password-reset-form">
                                                 <h2>Reset password</h2>
                                                 <form action="reset-password" method="post">
                                                     <input type="hidden" id="email" name="{{nameof(ResetPasswordModel.Email)}}" value = "{{email}}">
                                                     <input type="hidden" id="token" name="{{nameof(ResetPasswordModel.Token)}}" value = "{{token}}">
                                                     <div class="form-group">
                                                         <label for="password">New password:</label>
                                                         <input type="password" id="newPassword" name="{{nameof(ResetPasswordModel.NewPassword)}}" required>
                                                     </div>
                                                     <div class="form-group">
                                                         <label for="confirmPassword">Confirm password:</label>
                                                         <input type="password" id="confirmPassword" name="{{nameof(ResetPasswordModel.ConfirmPassword)}}" required>
                                                     </div>
                                                     <button type="submit">Reset</button>
                                                 </form>
                                             </div>
                                         </body>
                                         </html>
                                     """;
        
        return Content(resetPasswordPage, "text/html");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromForm]ResetPasswordModel resetPasswordModel, CancellationToken cancellationToken)
    {
        var dto = resetPasswordModel.ToDTO();
        if (dto is null)
        {
            return BadRequest();
        }

        var result = await userProfileService.ResetPasswordAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Errors);
    }

    [HttpGet("me")]
    [Authorize, ValidateSecurityStampFilter, ConfirmedEmailFilter]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await userProfileService.GetProfileInfoAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}