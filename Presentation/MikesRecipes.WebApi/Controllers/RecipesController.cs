using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikesRecipes.WebApi.Extensions;
using MikesRecipes.WebApi.ViewModels;
using System.ComponentModel.DataAnnotations;
using MikesRecipes.Application;
using MikesRecipes.Application.Contracts.Common;
using MikesRecipes.WebApi.Infrastructure.Filters;

namespace MikesRecipes.WebApi.Controllers;

[ApiVersion(Constants.WebApi.Version)]
[Authorize]
[ValidateSecurityStampFilter]
[ConfirmedEmailFilter]
public class RecipesController(IRecipeService recipeService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetRecipesPage([Range(1, int.MaxValue)] [FromQuery] int pageSize, [Range(1, int.MaxValue)] [FromQuery] int pageIndex, CancellationToken cancellationToken)
    {
        var result = await recipeService.GetAsync(new PagingOptions(pageIndex, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> GetRecipesPageByIncludedProducts([FromBody]ByIncludedProductsFilterModel filterModel, [Range(1, int.MaxValue)][FromQuery]int pageSize, [Range(1, int.MaxValue)][FromQuery]int pageIndex, CancellationToken cancellationToken)
    {
        var dto = filterModel.ToDTO();
        var result = await recipeService.GetByIncludedProductsAsync(dto, new PagingOptions(pageIndex, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}