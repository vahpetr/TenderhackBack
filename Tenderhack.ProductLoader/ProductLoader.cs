using System.Linq;
using System.Threading;
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
    private readonly ProductService _productService;
    private readonly ILogger<ProductLoader> _logger;

    public ProductLoader(TenderhackDbContext dbContext, ProductService productService, ILogger<ProductLoader> logger)
    {
      _dbContext = dbContext;
      _productService = productService;
      _logger = logger;
    }

    public async Task RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
      var externalIds = await _dbContext.Products
        .Select(p => p.ExternalId)
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);
      var externalIdsSet = externalIds.ToHashSet();
      var categories = await _dbContext.Categories
        .ToDictionaryAsync(p => p.Kpgz, cancellationToken)
        .ConfigureAwait(false);
      var characteristics = await _dbContext.Characteristics
        .ToDictionaryAsync(p => $"{p.Name}_{p.Value}", cancellationToken)
        .ConfigureAwait(false);

      // https://medium.com/@matias.paulo84/high-performance-csv-parser-with-system-io-pipelines-3678d4a5217a
      var parser = new ProductParser();
      var products = parser.Parse(args[0], externalIdsSet, categories, characteristics);

      var batchSize = 10000;
      var total = externalIds.Count;
      var savedBatchSize = (int)((float)total/(float)batchSize);
      var batchIndex = 0;

      foreach (var product in products)
      {
        _productService.AddItem(product);
        total++;
        _logger.LogInformation("Total: {Total}", total);

        if (++batchIndex == batchSize)
        {
          await _dbContext.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

          batchIndex = 0;
          savedBatchSize++;
          _logger.LogInformation("Total: {Total}, SavedBatchSize: {SavedBatchSize}", total, savedBatchSize);
        }
      }

      await _dbContext.SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);

      savedBatchSize++;
      _logger.LogInformation("Total: {Total}, SavedBatchSize: {SavedBatchSize}", total, savedBatchSize);
    }
  }
}
