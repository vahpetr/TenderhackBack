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
  public class OrderService
  {
    private readonly TenderhackDbContext _dbContext;

    public OrderService(TenderhackDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async IAsyncEnumerable<Order> GetItemsAsync(
      OrderFilter filter, Sorting<OrderSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = Query(_dbContext.Orders);

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

    public async Task<int> GetCountAsync(OrderFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Orders.AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Order?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await Query(_dbContext.Orders)
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public void AddItem(Order item)
    {
      _dbContext.Orders.Add(item);
    }

    public void UpdateItem(Order dbItem, Order item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);
    }

    public async Task<Order> SaveItemAsync(Order item, CancellationToken cancellationToken = default)
    {
      Order? dbItem = null;

      if (item.Id != 0)
      {
        dbItem = await GetItemAsync(item.Id, cancellationToken)
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

    public async Task<bool> ExistAsync(int id, CancellationToken cancellationToken = default)
    {
      return await _dbContext.Orders
        .AnyAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);
    }

    public async Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = new Order() { Id = id };

      _dbContext.Orders.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private static IQueryable<Order> ApplyFilter(IQueryable<Order> query, OrderFilter filter)
    {
      if (filter.Ids != null && filter.Ids.Count != 0)
      {
        var ids = filter.Ids;
        query = query.Where(p => ids.Contains(p.Id));
      }

      if (filter.ProductIds != null && filter.ProductIds.Count != 0)
      {
        var productIds = filter.ProductIds;
        query = query.Where(p => productIds.Contains(p.ProductId));
      }

      if (filter.ContractIds != null && filter.ContractIds.Count != 0)
      {
        var contractIds = filter.ContractIds;
        query = query.Where(p => contractIds.Contains(p.ContractId));
      }

      return query;
    }

    private static IQueryable<Order> ApplySoring(IQueryable<Order> query, Sorting<OrderSortType> sorting)
    {
      query = sorting.Sort switch
      {
        OrderSortType.Id => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Id),
          _ => query.OrderByDescending(p => p.Id)
        },
        OrderSortType.Amount => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Amount),
          _ => query.OrderByDescending(p => p.Amount)
        },
        OrderSortType.Quantity => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Quantity),
          _ => query.OrderByDescending(p => p.Quantity)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<Order> ApplyPaging(IQueryable<Order> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }

    private static IQueryable<Order> Query(IQueryable<Order> query)
    {
      return query.AsNoTracking();
    }
  }
}
