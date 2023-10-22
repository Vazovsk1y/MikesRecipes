using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MikesRecipes.Domain.Models;
using MikesRecipes.Services;
using MikesRecipes.Services.DTOs;
using MikesRecipes.Services.DTOs.Common;

namespace MikesRecipes.Web.Pages.Recipes
{
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

		public IEnumerable<RecipeDTO>? Recipes => RecipesPage?.Items;

		public async Task OnGetAsync(int pageIndex = 1)
		{
			var result = await _recipeService.GetAsync(new PagingOptions(pageIndex, DefaultPageSize));
			if (result.IsSuccess)
			{
				RecipesPage = result.Value;
			}
		}

		public async Task<IActionResult> OnPostBySelectedProductsAsync(string productsIdsRow)
		{
			if (!string.IsNullOrWhiteSpace(productsIdsRow))
			{
				var associatedProductsIds = new List<ProductId>();
                foreach (var item in productsIdsRow.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Guid.TryParse(item, out var value))
					{
						return BadRequest();
					}

					associatedProductsIds.Add(new ProductId(value));
                }

                var result = await _recipeService.GetByIncludedProductsAsync(associatedProductsIds);
				if (result.IsSuccess)
				{
					RecipesPage = result.Value;
				}
			}
			return Page();
		}
	}
}
