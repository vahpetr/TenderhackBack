using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Data.TenderhackDbContext.Models;
using Tenderhack.Core.Dto;
using Tenderhack.PredictQuantity.Model;

namespace Tenderhack.Core.Services
{
  public class PredictService
  {
    private static readonly Dictionary<int, string> SeasonsMap = new()
    {
      {1, "1Зима"},
      {2,"1Зима"},
      {3,"2Весна"},
      {4, "2Весна"},
      {5, "2Весна"},
      {6, "3Лето"},
      {7, "3Лето"},
      {8, "3Лето"},
      {9, "4Осень"},
      {10, "4Осень"},
      {11, "4Осень"},
      {12, "1Зима"},
    };

    private readonly Lazy<TenderhackDbContext> _dbContext;
    private readonly PredictionEnginePool<PredictQuantityInput, PredictQuantityOutput> _predictionEnginePool;

    public PredictService(
      Lazy<TenderhackDbContext> dbContext,
      PredictionEnginePool<PredictQuantityInput, PredictQuantityOutput> predictionEnginePool
      )
    {
      _dbContext = dbContext;
      _predictionEnginePool = predictionEnginePool;
    }

    /// <summary>
    /// Find optimal orders by purchases
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ScoredOrder> PredictDemandsAsync(
      PredictDemandsRequest request,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var now = DateTime.UtcNow;
      var month = now.Month;
      var inn = request.Inn;
      var kppRegion = request.Kpp.Substring(0, 2);
      var kppRegionNumber = int.Parse(kppRegion);
      var season = SeasonsMap[month];
      var minPublicAt = now.AddYears(request.YearOffset);
      var maxPublicAt = minPublicAt.AddYears(1);

      var items = Query(_dbContext.Value.Orders)
        .Where(p =>
            p.Contract.Customer.Inn == inn &&
            p.Contract.Customer.Kpp.StartsWith(kppRegion) &&
            p.Contract.PublicAt >= minPublicAt &&
            p.Contract.PublicAt < maxPublicAt
        )
        .OrderBy(p => p.Contract.PublicAt)
        .Take(request.Take)
        .AsAsyncEnumerable()
        .Select(order => new ScoredOrder()
        {
          Score = _predictionEnginePool.Predict("PredictQuantity", new PredictQuantityInput()
          {
            CustomerRegion = kppRegionNumber,
            Kpgz = order.Product.Category.Kpgz,
            Season = season
          }).Score,
          Order = order
        })
        .WithCancellation(cancellationToken)
        .ConfigureAwait(false);

      await foreach (var item in items)
      {
        yield return item;
      }
    }

    /// <summary>
    /// Find optimal orders by sells
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ScoredOrder> PredictPurchasesAsync(
      PredictPurchasesRequest request,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var now = DateTime.UtcNow;
      var month = now.Month;
      var inn = request.Inn;
      var kppRegion = request.Kpp.Substring(0, 2);
      var kppRegionNumber = int.Parse(kppRegion);
      var season = SeasonsMap[month];
      var minPublicAt = now.AddYears(request.YearOffset);
      var maxPublicAt = minPublicAt.AddYears(1);

      var items = Query(_dbContext.Value.Orders)
        .Where(p =>
          p.Contract.Producer.Inn == inn &&
          p.Contract.Producer.Kpp.StartsWith(kppRegion) &&
          p.Contract.PublicAt >= minPublicAt &&
          p.Contract.PublicAt < maxPublicAt
        )
        .OrderBy(p => p.Contract.PublicAt)
        .Take(request.Take)
        .AsAsyncEnumerable()
        .Select(order => new ScoredOrder()
        {
          Score = _predictionEnginePool.Predict("PredictQuantity", new PredictQuantityInput()
          {
            CustomerRegion = kppRegionNumber,
            Kpgz = order.Product.Category.Kpgz,
            Season = season
          }).Score,
          Order = order
        })
        .WithCancellation(cancellationToken)
        .ConfigureAwait(false);

      await foreach (var item in items)
      {
        yield return item;
      }
    }

    private static IQueryable<Order> Query(IQueryable<Order> query)
    {
      return query.AsNoTracking()
        .Include(p => p.Product)
          .ThenInclude(p => p.Category);
    }
  }
}
