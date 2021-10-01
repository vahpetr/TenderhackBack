using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Services;
using Tenderhack.ProductLoader;

var isDebug = args.Contains("--debug");
var connectionString =
  "Host=127.0.0.1;Port=5432;Username=tenderhack_user;Password=tenderhack_pass;Database=tenderhack_db";

var services = new ServiceCollection()
  .AddLogging(builder =>
  {
    builder.AddConsole();

    if (isDebug) {
      builder.AddDebug();
    }
  })
  .AddDbContext<TenderhackDbContext>(options =>
  {
    options
      .EnableSensitiveDataLogging(isDebug)
      .EnableDetailedErrors(isDebug);

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
      npgsqlOptions
        .CommandTimeout(60) // 60 sec
        .MinBatchSize(1)
        .MaxBatchSize(10000);
    });
  })
  .AddScoped<TenderhackDbContextMigrator>()
  .AddScoped<ProductService>()
  .AddScoped<PropertyService>()
  .AddScoped<ProductLoader>();

var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
await scope.ServiceProvider.GetRequiredService<TenderhackDbContextMigrator>().MigrateAsync().ConfigureAwait(false);
await scope.ServiceProvider.GetRequiredService<ProductLoader>().RunAsync(args).ConfigureAwait(false);
