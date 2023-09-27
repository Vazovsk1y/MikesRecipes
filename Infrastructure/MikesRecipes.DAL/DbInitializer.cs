using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MikesRecipes.Data;
using System.Diagnostics;

namespace MikesRecipes.DAL;

public class DbInitializer : IDbInitializer
{
	private readonly IApplicationDbContext _context;
	private readonly ILogger<DbInitializer> _logger;
	

	public DbInitializer(
		IApplicationDbContext context,
		ILogger<DbInitializer> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var timer = Stopwatch.StartNew();

		_logger.LogInformation("-----Start migration process------");
		await _context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
		_logger.LogInformation("------Times spend [{timerElapsedTotalSeconds}]-------", timer.Elapsed.TotalSeconds);
	}
}
