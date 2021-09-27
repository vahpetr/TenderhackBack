using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Services;

namespace Tenderhack.ContractLoader
{
  public class ContractLoader
  {
    private readonly TenderhackDbContext _dbContext;
    private readonly ContractService _contractService;
    private readonly ILogger<ContractLoader> _logger;

    public ContractLoader(TenderhackDbContext dbContext, ContractService contractService, ILogger<ContractLoader> logger)
    {
      _dbContext = dbContext;
      _contractService = contractService;
      _logger = logger;
    }

    public async Task RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
      var organizations = await _dbContext.Organizations
        .ToDictionaryAsync(p => $"{p.Inn}_{p.Kpp}", cancellationToken)
        .ConfigureAwait(false);
      var products = await _dbContext.Products
        .ToDictionaryAsync(p => p.ExternalId, p => p.Id, cancellationToken)
        .ConfigureAwait(false);

      // https://medium.com/@matias.paulo84/high-performance-csv-parser-with-system-io-pipelines-3678d4a5217a
      var parser = new ContractParser();
      var contracts = parser.Parse(args[0], organizations, products);

      var batchSize = 10000;
      var total = 0;
      var savedBatchSize = (int)((float)total/(float)batchSize);
      var batchIndex = 0;

      foreach (var contract in contracts)
      {
        _contractService.AddItem(contract);
        total++;
        _logger.LogInformation("Total: {Total}", total);

        if (++batchIndex == batchSize)
        {
          await _dbContext.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

          batchIndex = 0;
          savedBatchSize++;
          _logger.LogInformation("Total: {Count}, SavedBatchSize: {SavedBatchSize}", total, savedBatchSize);
        }
      }

      await _dbContext.SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);

      savedBatchSize++;
      _logger.LogInformation("Total: {Total}, SavedBatchSize: {SavedBatchSize}", total, savedBatchSize);
    }
  }
}
