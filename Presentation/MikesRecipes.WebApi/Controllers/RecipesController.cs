using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.Services;
using MikesRecipes.Services.Contracts.Common;
using MikesRecipes.WebApi.Constants;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(ApiVersions.V1Dot0)]
[Authorize]
public class RecipesController(IRecipeService recipeService) : BaseController
{
    private readonly IRecipeService _recipeService = recipeService;

    [HttpGet]
    public async Task<IActionResult> GetRecipesPage(
    [Range(1, int.MaxValue)]
    [FromQuery]
    int pageSize,
    [Range(1, int.MaxValue)]
    [FromQuery]
    int pageIndex,
    CancellationToken cancellationToken)
    {
        var result = await _recipeService.GetAsync(new PagingOptions(pageIndex, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> GetRecipesPageByIncludedProducts(
    [FromBody]
    ByIncludedProductsFilterModel filterModel,
    [Range(1, int.MaxValue)]
    [FromQuery]
    int pageSize,
    [Range(1, int.MaxValue)]
    [FromQuery]
    int pageIndex,
    CancellationToken cancellationToken)
    {
        var dto = filterModel.ToDTO();
        var result = await _recipeService.GetByIncludedProductsAsync(dto, new PagingOptions(pageIndex, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}