using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Filters;
using System.ComponentModel.DataAnnotations;
using MikesRecipes.Application;
using MikesRecipes.Application.Contracts.Requests;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(ApiVersions.V1Dot0)]
[Authorize]
[ValidateSecurityStampFilter]
[ConfirmedEmailFilter]
public class ProductsController(IProductService productService) : BaseController
{
    private readonly IProductService _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetByTitleSearchTerm([Required][FromQuery] string searchByTitleTerm, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByTitleAsync(new ByTitleFilter(searchByTitleTerm), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}