using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MikesRecipes.Application;
using MikesRecipes.Application.Contracts.Requests;
using MikesRecipes.WebApi.Infrastructure.Filters;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(Constants.WebApi.Version)]
[Authorize]
[ValidateSecurityStampFilter]
[ConfirmedEmailFilter]
public class ProductsController(IProductService productService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetByTitleSearchTerm([Required][FromQuery] string searchByTitleTerm, CancellationToken cancellationToken)
    {
        var result = await productService.GetByTitleAsync(new ByTitleFilter(searchByTitleTerm), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}