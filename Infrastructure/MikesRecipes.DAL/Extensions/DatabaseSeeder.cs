using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using MikesRecipes.Domain.Models;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Reflection;
using MikesRecipes.Domain.Constants;

namespace MikesRecipes.DAL.Extensions;

public static class DatabaseSeeder
{
    #region --Private classes--
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
    private class IngredientsConverter : TypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));

            text = text.Replace("None", "'По вкусу'");
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

            return result ?? new Dictionary<string, string>();
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            throw new NotImplementedException();
        }
    }
#nullable enable
    #endregion

    private const string RecipesFileResourseName = "MikesRecipes.DAL.povarenok_recipes_2021_06_16.csv";

    public static void Seed(this MikesRecipesDbContext dbContext, ILogger logger)
    {
        if (!dbContext.IsAbleToSeed())
        {
            return;
        }

        var stopwatch = new Stopwatch();

        logger.LogInformation("Data seeding started.");
        stopwatch.Start();

        dbContext.InsertData();
        dbContext.SaveChanges();

        stopwatch.Stop();
        logger.LogInformation("Data seeding ended. Times(seconds) elapsed {TotalSecondsElapsed}", stopwatch.Elapsed.TotalSeconds);
    }

    private static bool IsAbleToSeed(this MikesRecipesDbContext dbContext)
    {
        bool[] results = new[]
        {
            dbContext.Products.Any(),
            dbContext.Ingredients.Any(),
            dbContext.Products.Any(),
            dbContext.Roles.Any(),
        };

        return results.All(e => e is false);
    }

    private static void InsertData(this MikesRecipesDbContext dbContext)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RecipesFileResourseName);
        using var reader = new StreamReader(stream!);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

        var recipeItems = csvReader.GetRecords<RecipeItem>();
        var productsToInsert = new List<Product>();
        var recipesToInsert = new List<Recipe>();
        var ingredientsToInsert = new List<Ingredient>();

        foreach (var item in recipeItems)
        {
            if (item.Ingredients.Keys.Count == 0)
            {
                continue;
            }

            foreach (var productTitle in item.Ingredients.Keys)
            {
                if (!productsToInsert.Any(e => string.Equals(e.Title, productTitle, StringComparison.OrdinalIgnoreCase)))
                {
                    productsToInsert.Add(new Product { Title = productTitle });
                }
            }

            var recipe = new Recipe
            {
                Title = item.Name,
                Url = item.Url,
            };

            var recipeIngredients = productsToInsert
                .Where(e => item.Ingredients.Keys.Contains(e.Title, StringComparer.OrdinalIgnoreCase))
                .Select(e => new Ingredient { ProductId = e.Id, RecipeId = recipe.Id });

            recipe.IngredientsCount = recipeIngredients.Count();
            if (item.Ingredients.Count != recipe.IngredientsCount)
            {
                throw new InvalidDataException("Actual recipe ingredient count is not equal to recipeItem ingredient count.");
            }

            recipesToInsert.Add(recipe);
            ingredientsToInsert.AddRange(recipeIngredients);
        }


        dbContext.Products.AddRange(productsToInsert);
        dbContext.Recipes.AddRange(recipesToInsert);
        dbContext.Ingredients.AddRange(ingredientsToInsert);
        dbContext.AddRoles();
    }

    private static void AddRoles(this MikesRecipesDbContext dbContext)
    {
        var roles = new Role[] {
        new()
        {
           ConcurrencyStamp = Guid.NewGuid().ToString(),
           Name = DefaultRoles.Admin,
           NormalizedName = DefaultRoles.Admin.ToUpper(),
        },
        new()
        {
           ConcurrencyStamp = Guid.NewGuid().ToString(),
           Name = DefaultRoles.User,
           NormalizedName = DefaultRoles.User.ToUpper(),
        }};

        dbContext.Roles.AddRange(roles);
    }
}
