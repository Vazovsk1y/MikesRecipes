using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MikesRecipes.Domain.Models;
using MikesRecipes.Services;
using MikesRecipes.Services.DTOs;

namespace MikesRecipes.Web.Pages.Recipes
{
    public class IndexModel : PageModel
    {
        private readonly IRecipeService _recipeService;

		public IndexModel(IRecipeService recipeService)
		{
			_recipeService = recipeService;
		}

		public IEnumerable<RecipeDTO>? Recipes { get; set; }

        public void OnGet() 
        {

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

                var result = await _recipeService.GetAsync(associatedProductsIds);
				if (result.IsSuccess)
				{
					Recipes = result.Value.Recipes;
				}
			}
			return Page();
		}
	}
}
