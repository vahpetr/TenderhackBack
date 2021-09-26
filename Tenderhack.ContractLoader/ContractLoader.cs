using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Services;

namespace Tenderhack.ContractLoader
{
  public class ContractLoader
  {
    private readonly TenderhackDbContext _dbContext;
    private readonly TenderhackDbContextMigrator _migrator;
    private readonly ContractService _contractService;
    private readonly ILogger<ContractLoader> _logger;

    public ContractLoader(TenderhackDbContext dbContext, TenderhackDbContextMigrator migrator, ContractService contractService, ILogger<ContractLoader> logger)
    {
      _dbContext = dbContext;
      _migrator = migrator;
      _contractService = contractService;
      _logger = logger;
    }

    public async Task Run(string[] args)
    {
      await _migrator.MigrateAsync();
      var organizations = _dbContext.Organizations.ToDictionary(p => $"{p.Inn}_{p.Kpp}");
      var productMap = _dbContext.Products.ToDictionary(p => p.ExternalId, p => p.Id);

      // https://medium.com/@matias.paulo84/high-performance-csv-parser-with-system-io-pipelines-3678d4a5217a
      var parser = new ContractParser();
      var contracts = parser.Parse(args[0], organizations, productMap);

      // var i = 0;
      // foreach (var c in contracts)
      // {
      //   Console.WriteLine($"{c.Number} {c.PublicAt} {c.ConclusionAt} {c.Price}");
      //   Console.WriteLine($" - {c.Customer.Inn} {c.Customer.Kpp} {c.Customer.Name}");
      //   Console.WriteLine($" - {c.Provider.Inn} {c.Provider.Kpp} {c.Provider.Name}");

      //   foreach (var o in c.Orders)
      //   {
      //     var productId = o.ProductId.HasValue ? $"{o.ProductId.Value}" : "null";
      //     Console.WriteLine($" - - {productId} {o.Quantity} {o.Amount}");
      //   }
      //   Console.WriteLine(++i);
      // }

      var batchSize = 1;//10000;
      var count = 0;//externalIds.Count;
      var savedBatchSize = (int)((float)count/(float)batchSize);
      var batchIndex = 0;
      foreach (var c in contracts)
      {
        _contractService.AddItem(c);
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
