using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandomRecipes.Data;
using RandomRecipes.Domain.Models;
using RandomRecipes.Domain.Services;
using System.Diagnostics;

namespace RandomRecipes.DAL;

public class DbInitializer : IDbInitializer
{
	private readonly IApplicationDbContext _context;
	private readonly ILogger<DbInitializer> _logger;
	private readonly ICsvParser<Product> _csvParser;
	private const string SeedCsvFilePath = @"C:\Users\andrey\Downloads\foodb_2020_4_7_csv\foodb_2020_04_07_csv\Food.csv";

	public DbInitializer(
		IApplicationDbContext context,
		ILogger<DbInitializer> logger,
		ICsvParser<Product> csvParser)
	{
		_context = context;
		_logger = logger;
		_csvParser = csvParser;
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var timer = Stopwatch.StartNew();

		_logger.LogInformation("-----Start migration process------");
		await _context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
		_logger.LogInformation("------Times spend [{timerElapsedTotalSeconds}]-------", timer.Elapsed.TotalSeconds);
	}

	public async Task SeedDataAsync(CancellationToken cancellationToken = default)
	{
		await _context.Products.AddRangeAsync(_csvParser.Enumerate(SeedCsvFilePath), cancellationToken).ConfigureAwait(false);
		await _context.SaveChangesAsync(cancellationToken);
	}
}
