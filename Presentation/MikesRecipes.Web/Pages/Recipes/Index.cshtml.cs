using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MikesRecipes.Domain.Models;
using MikesRecipes.Services;
using MikesRecipes.Services.Contracts;
using MikesRecipes.Services.Contracts.Common;

namespace MikesRecipes.Web.Pages.Recipes;

[Authorize]
public class IndexModel : PageModel
{
	private readonly IRecipeService _recipeService;
	private readonly int DefaultPageSize;

	public IndexModel(IRecipeService recipeService, IConfiguration configuration)
	{
		DefaultPageSize = configuration.GetValue<int>(nameof(DefaultPageSize));
		_recipeService = recipeService;
	}

	public RecipesPage? RecipesPage { get; set; }

	public IEnumerable<RecipeDTO> Recipes => RecipesPage?.Items ?? Enumerable.Empty<RecipeDTO>();

	[BindProperty]
	public InputModel Input { get; set; } = new InputModel();

	public async Task OnGetAsync(int pageIndex = 1)
	{
		var result = await _recipeService.GetAsync(new PagingOptions(pageIndex, DefaultPageSize));
		if (result.IsSuccess)
		{
			RecipesPage = result.Value;
		}
	}

	public async Task<IActionResult> OnPostBySelectedProductsAsync(InputModel input)
	{
		Input = input;
		if (!string.IsNullOrWhiteSpace(Input.ProductsIdsRow))
		{
			var associatedProductsIds = new List<ProductId>();
			foreach (var item in Input.ProductsIdsRow.Split(',', StringSplitOptions.RemoveEmptyEntries))
			{
				if (!Guid.TryParse(item, out var value))
				{
					return BadRequest();
				}

				associatedProductsIds.Add(new ProductId(value));
			}

			var result = await _recipeService.GetByIncludedProductsAsync(associatedProductsIds, Input.OtherProductsCount, new PagingOptions(Input.PageIndex, DefaultPageSize));
			if (result.IsSuccess)
			{
				RecipesPage = result.Value;
			}
		}
		return Page();
	}

	public class InputModel
	{
		public string? ProductsIdsRow { get; set; }

		public int OtherProductsCount { get; set; }

		public int PageIndex { get; set; }
	}
}
