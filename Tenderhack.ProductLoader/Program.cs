using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Services;
using Tenderhack.ProductLoader;

var services = new ServiceCollection()
  .AddLogging(builder =>
  {
    builder.AddConsole();
    builder.AddDebug();
  })
  .AddDbContext<TenderhackDbContext>(options =>
  {
    options
    .EnableSensitiveDataLogging(false)
    .EnableDetailedErrors(false);

    var connectionString =
      "Host=127.0.0.1;Port=5432;Username=tenderhack_user;Password=tenderhack_pass;Database=tenderhack_db";
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
  .AddScoped<CharacteristicService>()
  .AddScoped<ProductLoader>();

var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
await scope.ServiceProvider.GetRequiredService<ProductLoader>().Run(args);
