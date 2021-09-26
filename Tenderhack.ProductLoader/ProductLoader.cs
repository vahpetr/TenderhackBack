using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Services;

namespace Tenderhack.ProductLoader
{
  public class ProductLoader
  {
    private readonly TenderhackDbContext _dbContext;
    private readonly TenderhackDbContextMigrator _migrator;
    private readonly ProductService _productService;
    private readonly ILogger<ProductLoader> _logger;

    public ProductLoader(TenderhackDbContext dbContext, TenderhackDbContextMigrator migrator, ProductService productService, ILogger<ProductLoader> logger)
    {
      _dbContext = dbContext;
      _migrator = migrator;
      _productService = productService;
      _logger = logger;
    }

    public async Task Run(string[] args)
    {
      await _migrator.MigrateAsync();
      var externalIds = await _dbContext.Products.Select(p => p.ExternalId).ToListAsync();
      var externalIdsSet = externalIds.ToHashSet();
      var categories = _dbContext.Categories.ToDictionary(p => p.Kpgz);
      var characteristics = _dbContext.Characteristics.ToDictionary(p => $"{p.Name}_{p.Value}");
      // https://medium.com/@matias.paulo84/high-performance-csv-parser-with-system-io-pipelines-3678d4a5217a
      var parser = new ProductParser();
      var products = parser.Parse(args[0], externalIdsSet, categories, characteristics);

      // var i = 0;
      // foreach (var p in products)
      // {
      //   Console.WriteLine($"{p.ExternalId} {p.Category.Title} {p.CpgzCode}");
      //
      //   foreach (var c in p.Characteristics)
      //   {
      //     Console.WriteLine($" - {c.Id} {c.Name} {c.Value}");
      //   }
      //   Console.WriteLine(++i);
      // }

      var batchSize = 10000;
      var count = externalIds.Count;
      var savedBatchSize = (int)((float)count/(float)batchSize);
      var batchIndex = 0;
      foreach (var p in products)
      {
        _productService.AddItem(p);
        count++;
        Console.WriteLine($"index: {count}");
        if (++batchIndex == batchSize)
        {
          await _dbContext.SaveChangesAsync();
          batchIndex = 0;
          savedBatchSize++;
          Console.WriteLine($"index: {count}, savedBatchSize: {savedBatchSize}");
        }
      }
    }
  }
}
