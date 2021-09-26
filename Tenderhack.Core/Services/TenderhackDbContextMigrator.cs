using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Data.TenderhackDbContext;

namespace Tenderhack.Core.Services
{
  public class TenderhackDbContextMigrator
  {
    private const string DbContextName = nameof(TenderhackDbContext);

    private readonly TenderhackDbContext _dbContext;
    private readonly ILogger<TenderhackDbContextMigrator> _logger;

    public TenderhackDbContextMigrator(TenderhackDbContext dbContext, ILogger<TenderhackDbContextMigrator> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    private Task SeedAsync(CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    private async Task TransactionSeedAsync(CancellationToken cancellationToken = default)
    {
      var context = (DbContext)_dbContext;
      await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

      try
      {
        await SeedAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
      }
      catch (Exception)
      {
        await transaction.RollbackAsync(cancellationToken);
      }
    }

    public async Task SeedAsync(int timeout = 300, CancellationToken cancellationToken = default)
    {
      _logger.LogWarning("Database \"{DbContextName}\" seeding started", DbContextName);

      try
      {
        var context = (DbContext)_dbContext;
        var originalTimeout = context.Database.GetCommandTimeout();

        context.Database.SetCommandTimeout(timeout);

        await TransactionSeedAsync(cancellationToken);

        context.Database.SetCommandTimeout(originalTimeout);

        _logger.LogWarning("Database \"{DbContextName}\" seeding completed", DbContextName);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An unexpected error occurred during database \"{DbContextName}\" seeding", DbContextName);
        throw;
      }
    }

    public async Task MigrateAsync(int timeout = 60, CancellationToken cancellationToken = default)
    {
      _logger.LogWarning("Database \"{DbContextName}\" migration started", DbContextName);

      try
      {
        var context = (DbContext)_dbContext;
        var originalTimeout = context.Database.GetCommandTimeout();

        var migrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken))
          .ToList();
        if (migrations.Count > 0)
        {
          context.Database.SetCommandTimeout(timeout);

          await context.Database.MigrateAsync(cancellationToken);

          context.Database.SetCommandTimeout(originalTimeout);

          foreach (var migration in migrations)
          {
            _logger.LogWarning("Database \"{DbContextName}\" migration \"{Migration}\" completed", DbContextName,
              migration);
          }

          _logger.LogWarning("All database \"{DbContextName}\" migrations completed", DbContextName);
        }
        else
        {
          _logger.LogWarning("No new migrations in database \"{DbContextName}\"", DbContextName);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An unexpected error occurred during database \"{DbContextName}\" migration",
          DbContextName);
        throw;
      }
    }
  }

}
