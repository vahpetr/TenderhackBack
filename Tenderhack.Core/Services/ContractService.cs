using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tenderhack.Core.Data.TenderhackDbContext;
using Tenderhack.Core.Data.TenderhackDbContext.Models;
using Tenderhack.Core.Dto;
using Tenderhack.Core.Types;

namespace Tenderhack.Core.Services
{
  public class ContractService
  {
    private readonly TenderhackDbContext _dbContext;
    private readonly OrderService _orderService;

    public ContractService(TenderhackDbContext dbContext, OrderService orderService)
    {
      _dbContext = dbContext;
      _orderService = orderService;
    }

    public async IAsyncEnumerable<Contract> GetItemsAsync(
      ContractFilter filter, Sorting<ContractSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = _dbContext.Contracts
        .Include(p => p.Customer)
        .Include(p => p.Provider)
        .Include(p => p.Orders.OrderBy(t => t.Id))
        .AsNoTracking();

      query = ApplyFilter(query, filter);

      query = ApplySoring(query, sorting);

      query = ApplyPaging(query, paging);

      var items = query
        .AsAsyncEnumerable()
        .WithCancellation(cancellationToken)
        .ConfigureAwait(false);

      await foreach (var item in items)
      {
        yield return item;
      }
    }

    public async Task<int> GetCountAsync(ContractFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Contracts
        .AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Contract?> FindItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Contracts
        .FindAsync(new object[] { id }, cancellationToken)
        .ConfigureAwait(false);

      if (item == null)
      {
        return null;
      }

      await _dbContext.Entry(item)
        .Collection(p => p.Orders.OrderBy(t => t.Id))
        .LoadAsync(cancellationToken)
        .ConfigureAwait(false);

      await _dbContext.Entry(item)
        .Reference(b => b.Customer)
        .LoadAsync(cancellationToken)
        .ConfigureAwait(false);

      await _dbContext.Entry(item)
        .Reference(b => b.Provider)
        .LoadAsync(cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public async Task<Contract?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Contracts
        .AsNoTracking()
        .Include(p => p.Customer)
        .Include(p => p.Provider)
        .Include(p => p.Orders.OrderBy(t => t.Id))
        .AsSplitQuery()
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      if (item == null)
      {
        return null;
      }

      return item;
    }

    public void AddItem(Contract item)
    {
      foreach (var tag in item.Orders)
      {
        if (tag.Id == 0)
        {
          _orderService.AddItem(tag);
        }
        else
        {
          _dbContext.Entry(tag).State = EntityState.Unchanged;
        }
      }

      _dbContext.Contracts.Add(item);
    }

    public void UpdateItem(Contract dbItem, Contract item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);

      var dbTagMap = dbItem.Orders.ToDictionary(p => p.Id);
      foreach (var tag in item.Orders)
      {
        if (dbTagMap.TryGetValue(tag.Id, out var dbTag))
        {
          // update
          _orderService.UpdateItem(dbTag, tag);
        }
        else
        {
          if (tag.Id == 0)
          {
            _orderService.AddItem(tag);
          }
          else
          {
            _dbContext.Entry(tag).State = EntityState.Unchanged;
          }

          // attach
          dbItem.Orders.Add(tag);
        }
      }

      var tagMap = item.Orders.ToDictionary(p => p.Id);
      foreach (var dbTag in dbItem.Orders)
      {
        if (!tagMap.ContainsKey(dbTag.Id))
        {
          // detach
          dbItem.Orders.Remove(dbTag);
        }
      }
    }

    public async Task<Contract> SaveItemAsync(Contract item, CancellationToken cancellationToken = default)
    {
      Contract? dbItem = null;

      if (item.Id != 0)
      {
        dbItem = await FindItemAsync(item.Id, cancellationToken)
          .ConfigureAwait(false);
      }

      if (dbItem == null)
      {
        AddItem(item);

        await _dbContext
          .SaveChangesAsync(cancellationToken)
          .ConfigureAwait(false);

        return item;
      }

      UpdateItem(dbItem, item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);

      return dbItem;
    }

    public async Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = new Contract() { Id = id };

      _dbContext.Contracts.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private IQueryable<Contract> ApplyFilter(IQueryable<Contract> query, ContractFilter filter)
    {
      if (filter.Ids != null && filter.Ids.Count != 0)
      {
        var ids = filter.Ids;
        query = query.Where(p => ids.Contains(p.Id));
      }

      if (filter.MinPublicAt.HasValue)
      {
        var minPublicAt = filter.MinPublicAt.Value;
        query = query.Where(p => p.PublicAt >= minPublicAt);
      }

      if (filter.MaxPublicAt.HasValue)
      {
        var maxPublicAt = filter.MaxPublicAt.Value;
        query = query.Where(p => p.PublicAt < maxPublicAt);
      }

      if (filter.MinConclusionAt.HasValue)
      {
        var minConclusionAt = filter.MinConclusionAt.Value;
        query = query.Where(p => p.ConclusionAt.HasValue && p.ConclusionAt >= minConclusionAt);
      }

      if (filter.MaxConclusionAt.HasValue)
      {
        var maxConclusionAt = filter.MaxConclusionAt.Value;
        query = query.Where(p => p.ConclusionAt.HasValue &&  p.ConclusionAt < maxConclusionAt);
      }

      if (filter.MinPrice.HasValue)
      {
        var minPrice = filter.MinPrice.Value;
        query = query.Where(p => p.Price >= minPrice);
      }

      if (filter.MaxPrice.HasValue)
      {
        var maxPrice = filter.MaxPrice.Value;
        query = query.Where(p => p.Price < maxPrice);
      }

      if (filter.CustomerIds != null && filter.CustomerIds.Count != 0)
      {
        var customerIds = filter.CustomerIds;
        query = query.Where(p => customerIds.Contains(p.CustomerId));
      }

      if (filter.ProviderIds != null && filter.ProviderIds.Count != 0)
      {
        var providerIds = filter.ProviderIds;
        query = query.Where(p => p.ProviderId.HasValue && providerIds.Contains(p.ProviderId.Value));
      }

      var q = filter.Q?.Trim().ToLower();
      if (q?.Length >= 3)
      {
        var like = $"%{q}%";
        query = query.Where(p => EF.Functions.ILike(p.Number, like));
      }

      return query;
    }

    private static IQueryable<Contract> ApplySoring(IQueryable<Contract> query, Sorting<ContractSortType> sorting)
    {
      query = sorting.Sort switch
      {
        ContractSortType.Id => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Id),
          _ => query.OrderByDescending(p => p.Id)
        },
        ContractSortType.Number => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Number),
          _ => query.OrderByDescending(p => p.Number)
        },
        ContractSortType.Price => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Price),
          _ => query.OrderByDescending(p => p.Price)
        },
        ContractSortType.ConclusionAt => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.ConclusionAt),
          _ => query.OrderByDescending(p => p.ConclusionAt)
        },
        ContractSortType.PublicAt => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.PublicAt),
          _ => query.OrderByDescending(p => p.PublicAt)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<Contract> ApplyPaging(IQueryable<Contract> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }
  }
}
