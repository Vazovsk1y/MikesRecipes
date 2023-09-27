using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using MikesRecipes.Data;
using MikesRecipes.Domain.Models;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace MikesRecipes.DAL;

public class DataSeeder : IDataSeeder
{
#nullable disable
	private class RecipeItem
	{
		[Name("url")]
		public string Url { get; set; }

		[Name("name")]
		public string Name { get; set; }

		[Name("ingredients")]
		[TypeConverter(typeof(IngredientsConverter))]
		public Dictionary<string, string> Ingredients { get; set; }   // first string product name, second required count.
	}
#nullable enable

	private class IngredientsConverter : TypeConverter
	{
		public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
		{
			ArgumentNullException.ThrowIfNull(text, nameof(text));

			text = text.Replace("None", "'По вкусу'");
			var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

			return result ?? new Dictionary<string, string>();
		}

		public override string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
		{
			throw new NotImplementedException();
		}
	}

	private readonly IApplicationDbContext _dbContext;
	private const string ProductsFilePath = @"C:\Users\andrey\Desktop\RecipesData\products-ru.txt";
	private const string RecipesFilePath = @"C:\Users\andrey\Desktop\RecipesData\povarenok_recipes_2021_06_16.csv";

	public DataSeeder(IApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task SeedDataAsync(CancellationToken cancellationToken = default)
	{
		await AddProducts(cancellationToken);
		await AddRecipes(cancellationToken);
	}

	private async Task AddProducts(CancellationToken cancellationToken)
	{
		if (_dbContext.Products.Any())
		{
			return;
		}

		await _dbContext.Products.BulkInsertAsync(GetProducts(), cancellationToken);
	}

	private async Task AddRecipes(CancellationToken cancellationToken)
	{
		if (_dbContext.Recipes.Any())
		{
			return;
		}

		var recipes = await GetRecipes().ConfigureAwait(false);
		await _dbContext.Recipes.BulkInsertAsync(recipes, cancellationToken);
		await _dbContext.Ingredients.BulkInsertAsync(recipes.SelectMany(e => e.Ingredients), cancellationToken);
    }

	private async Task<IEnumerable<Recipe>> GetRecipes()
	{
		using var reader = new StreamReader(File.OpenRead(RecipesFilePath));
		using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

		var recipeItems = csvReader.GetRecords<RecipeItem>().ToList();
		var existingProductsTitles = _dbContext.Products.Select(x => x.Title);
		var recipeItemsProductsTitles = recipeItems.SelectMany(e => e.Ingredients.Keys).Distinct(StringComparer.OrdinalIgnoreCase);

		var absentProductsTitles = recipeItemsProductsTitles.Except(existingProductsTitles, StringComparer.OrdinalIgnoreCase);
		if (absentProductsTitles.Any())
		{
			var productsToAdd = absentProductsTitles.Select(e => new Product
			{
				Title = e,
			});
			await _dbContext.Products.BulkInsertAsync(productsToAdd);
		}

		var result = new List<Recipe>();
		var existingProducts = await _dbContext.Products
			.ToListAsync();

		foreach (var item in recipeItems)
		{
			if (item.Ingredients.Keys.Count == 0)
			{
				continue;
			}

			var recipe = new Recipe
			{
				Title = item.Name,
				Url = item.Url,
			};

			recipe.Ingredients = existingProducts
				.Where(e => item.Ingredients.Keys.Contains(e.Title, StringComparer.OrdinalIgnoreCase))
				.Select(e => new Ingredient { ProductId = e.Id, RecipeId = recipe.Id })
				.ToList();

			if (item.Ingredients.Count != recipe.Ingredients.Count)
			{
				throw new InvalidDataException("Actual recipe ingredient count is not equal to recipeItem ingredient count.");
			}

			result.Add(recipe);
		}

		return result;
	}

	private static IEnumerable<Product> GetProducts()
	{
		using var reader = new StreamReader(File.OpenRead(ProductsFilePath));

		string? line;

		while ((line = reader.ReadLine()) != null)
		{
			yield return new Product
			{
				Title = line,
			};
		}
	}
}
