using CsvHelper;
using CsvHelper.Configuration.Attributes;
using RandomRecipes.Domain.Models;
using System.Globalization;

namespace RandomRecipes.Domain.Services.Implementations;

public class ProductCsvParser : ICsvParser<Product>
{
#nullable disable
	private class ProductItem
	{
		[Name("id")]
		public int Id { get; set; }

		[Name("name")]
		public string Name { get; set; }

		[Name("name_scientific")]
		public string ScientificName { get; set; }

		[Name("description")]
		public string Description { get; set; }

		[Name("itis_id")]
		public string ItisId { get; set; }

		[Name("wikipedia_id")]
		public string WikipediaId { get; set; }

		[Name("picture_file_name")]
		public string PictureFileName { get; set; }

		[Name("picture_content_type")]
		public string PictureContentType { get; set; }

		[Name("picture_file_size")]
		public int? PictureFileSize { get; set; }

		[Name("picture_updated_at")]
		[Format("yyyy-MM-dd HH:mm:ss UTC")]
		public DateTime? PictureUpdatedAt { get; set; }

		[Name("legacy_id")]
		public int? LegacyId { get; set; }

		[Name("food_group")]
		public string FoodGroup { get; set; }

		[Name("food_subgroup")]
		public string FoodSubgroup { get; set; }

		[Name("food_type")]
		public string FoodType { get; set; }

		[Name("created_at")]
		[Format("yyyy-MM-dd HH:mm:ss UTC")]
		public DateTime CreatedAt { get; set; }

		[Name("updated_at")]
		[Format("yyyy-MM-dd HH:mm:ss UTC")]
		public DateTime UpdatedAt { get; set; }

		[Name("creator_id")]
		public int? CreatorId { get; set; }

		[Name("updater_id")]
		public int? UpdaterId { get; set; }

		[Name("export_to_afcdb")]
		public bool ExportToAfcdb { get; set; }

		[Name("category")]
		public string Category { get; set; }

		[Name("ncbi_taxonomy_id")]
		public int? NcbiTaxonomyId { get; set; }

		[Name("export_to_foodb")]
		public bool ExportToFoodb { get; set; }

		[Name("public_id")]
		public string PublicId { get; set; }
	}
#nullable enable

	private const string DishesType = "Dishes";
	private const string ConfectioneriesType = "Confectioneries";

	public async IAsyncEnumerable<Product> EnumerateAsync(string csvFilePath)
	{
		ThrowIfFileIsNotExists(csvFilePath);

		using var reader = new StreamReader(csvFilePath);
		using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

		await foreach (var item in csvReader.GetRecordsAsync<ProductItem>())
		{
			if (IsValid(item))
			{
				yield return new Product
				{
					Title = item.Name
				};
			}
		}
	}

	public IEnumerable<Product> Enumerate(string csvFilePath)
	{
		ThrowIfFileIsNotExists(csvFilePath);

		using var reader = new StreamReader(csvFilePath);
		using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

		foreach (var item in csvReader.GetRecords<ProductItem>())
		{
			if (IsValid(item))
			{
				yield return new Product
				{
					Title = item.Name
				};
			}
		}
	}

	private static void ThrowIfFileIsNotExists(string path)
	{
		if (!File.Exists(path))
		{
			throw new InvalidOperationException("File isn't exists.");
		}
	}

	private static bool IsValid(ProductItem productItem)
	{
		return productItem.FoodGroup != DishesType && productItem.FoodGroup != ConfectioneriesType;
	}
}
