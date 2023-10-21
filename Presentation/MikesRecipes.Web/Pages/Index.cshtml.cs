using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MikesRecipes.Services;
using System.Net;

namespace MikesRecipes.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IProductService _productService;

    [BindProperty]
    public string? ProductsIdsRow { get; set; }

    public IndexModel(
        ILogger<IndexModel> logger,
        IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public void OnGet()
    {

    }

	public async Task<IActionResult> OnPostProductsAutoCompleteAsync(string productTitlePrefix)
    {
        var result = await _productService.GetAsync(productTitlePrefix);
        if (result.IsSuccess)
        {
            var response = new JsonResult(result.Value.Products);
            return response;
        }

        return Page();
    }
}
