using ESS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ESS.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbInitializer> _logger;
    private const int MaxRetries = 5;
    private const int RetryDelayMilliseconds = 5000; // 5 seconds

    public DbInitializer(
        ApplicationDbContext context,
        ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (!_context.Database.IsNpgsql())
        {
            _logger.LogWarning("Database is not PostgreSQL. Skipping initialization.");
            return;
        }

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Checking database connection... (Attempt {Attempt}/{MaxRetries})", attempt, MaxRetries);

                // Test if we can connect to the database
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Database connection successful. Applying migrations...");

                    // Apply migrations
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Migrations applied successfully");
                    return;
                }
                else
                {
                    // If this is the last attempt, log a warning
                    if (attempt == MaxRetries)
                    {
                        _logger.LogWarning("Cannot connect to the database after {MaxRetries} attempts. Migrations not applied.", MaxRetries);
                        return;
                    }

                    _logger.LogInformation("Database not ready. Waiting before retry...");
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }
            catch (Exception ex)
            {
                // If this is a "database is starting up" error, retry
                if (ex is Npgsql.PostgresException pgEx && pgEx.SqlState == "57P03")
                {
                    if (attempt < MaxRetries)
                    {
                        _logger.LogInformation("Database is starting up. Waiting before retry...");
                        await Task.Delay(RetryDelayMilliseconds);
                    }
                    else
                    {
                        _logger.LogError(ex, "Database still starting up after {MaxRetries} attempts. Initialization aborted.", MaxRetries);
                    }
                }
                else
                {
                    _logger.LogError(ex, "An error occurred while initializing the database");
                    // Only throw on the last attempt
                    if (attempt == MaxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }
        }
    }
}