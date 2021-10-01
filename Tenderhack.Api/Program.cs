using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Npgsql;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.DependencyInjection;
using Prometheus.Client.HttpRequestDurations;
using Tenderhack.Api.Json;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.DI;
using Tenderhack.Core.Services;
using Tenderhack.PredictQuantity.Model;

// if (args.Length == 0) throw new ArgumentException("Please setup startup arguments");

string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(c =>
{
    c.AddServerHeader = false;
});

// Add services to the container.
var services = builder.Services;
services.AddControllers();
services.AddSwaggerGen(c =>
{
    var filePath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
    if (!File.Exists(filePath))
    {
        throw new Exception("Assembly not found");
    }

    c.IncludeXmlComments(filePath);
    c.SwaggerDoc("v1", new() { Title = assemblyName, Version = "v1" });
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();

    c.DescribeAllParametersInCamelCase();

    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/1843/files
    c.UseInlineDefinitionsForEnums();

});
services.AddMetricFactory();
services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
var connectionString = GetConnectionString(builder.Configuration);
services.AddDbContextPool<TenderhackDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions
            .CommandTimeout(60) // 60 sec
            .MinBatchSize(1)
            .MaxBatchSize(1000);
    });
}, 1024);
services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 128 * 1024;
});
services.AddCors(options =>
  {
    options.AddDefaultPolicy(builder =>
      {
        builder
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
      });
  });
services.AddControllers(options =>
  {
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();

    options.CacheProfiles.Add("Caching",
      new CacheProfile
      {
        NoStore = true,
        Duration = 600,
        Location = ResponseCacheLocation.None
      });
    options.CacheProfiles.Add("NoCaching",
      new CacheProfile
      {
        NoStore = false,
        Duration = 0,
        Location = ResponseCacheLocation.Any
      });
  })
  .AddJsonOptions(options => SetupJsonSerializerOptions(options.JsonSerializerOptions));

services.AddScoped<TenderhackDbContextMigrator>();

services.AddTransient(typeof(Lazy<>), typeof(LazyLoader<>))
  .AddTransient<PropertyService>()
  .AddTransient<ContractService>()
  .AddTransient<OrderService>()
  .AddTransient<OrganizationService>()
  .AddTransient<ProductService>()
  .AddTransient<PredictService>();

var predictQuantityModelPath = Path.Join(Directory.GetCurrentDirectory(), "..", "Tenderhack.PredictQuantity.Model", "MLModel.zip");
services.AddPredictionEnginePool<PredictQuantityInput, PredictQuantityOutput>()
  .FromFile(modelName: "PredictQuantity", filePath:predictQuantityModelPath, watchForChanges: false);

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseDeveloperExceptionPage();
} else  {
  app.UseHsts();
}

app
    .UsePrometheusServer(options => {
        options.MapPath = "/metrics";
    })
    .UsePrometheusRequestDurations(options =>
    {
        options.IncludePath = true;
        options.IncludeMethod = true;
        options.IncludeStatusCode = true;
        options.IgnoreRoutesConcrete = new[]
        {
            "/",
            "/favicon.ico",
            "/robots.txt"
        };
        options.IgnoreRoutesStartWith = new[]
        {
            "/swagger",
            "/docs"
        };
        options.UseRouteName = true;
    })
    .UseHttpsRedirection()
    .UseSwagger()
    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{assemblyName} v1"))
    .UseReDoc(c =>
    {
        c.RoutePrefix = "docs";
        c.DocumentTitle = $"{assemblyName} API Docs";
    })
    .UseRouting()
    .UseCors()
    .UseAuthorization()
    .UseResponseCaching()
    .UseResponseCompression()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/health", context => Task.CompletedTask);
        endpoints.MapControllers();
    });

if (args.Contains("--migrate"))
{
    const int timeout = 60;
    using var scope = app.Services.CreateScope();
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeout * 1000);
        await scope.ServiceProvider.GetRequiredService<TenderhackDbContextMigrator>()
            .MigrateAsync(timeout, cts.Token)
            .ConfigureAwait(false);
    }
}

if (args.Contains("--seed"))
{
    const int timeout = 300;
    using var scope = app.Services.CreateScope();
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeout * 1000);
        await scope.ServiceProvider.GetRequiredService<TenderhackDbContextMigrator>()
            .SeedAsync(timeout, cts.Token)
            .ConfigureAwait(false);
    }
}

if (args.Any(p => p == "--serve"))
{
    app.Run();
}

static string GetConnectionString(IConfiguration cfg)
{
    var rawConnectionString = cfg.GetConnectionString(nameof(TenderhackDbContext));
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(rawConnectionString) && string.IsNullOrEmpty(databaseUrl))
        throw new ArgumentException(
            $"Connection string \"{nameof(TenderhackDbContext)}\" and database url \"DATABASE_URL\" not set");

    var connectionString = !string.IsNullOrEmpty(databaseUrl) ? databaseUrl : rawConnectionString;
    if (!connectionString.Contains(';'))
    {
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/')
        };
        connectionString = builder.ToString();
    }
    return connectionString;
}

static void SetupJsonSerializerOptions(JsonSerializerOptions options)
{
    var policy = JsonNamingPolicy.CamelCase;
    options.PropertyNamingPolicy = policy;
    options.DictionaryKeyPolicy = policy;
    options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

    options.Converters.Add(new JsonStringEnumConverter(policy));
    options.Converters.Add(new UtczDateTimeConverter());
}
