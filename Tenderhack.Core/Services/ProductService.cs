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
  public class ProductService
  {
    private readonly TenderhackDbContext _dbContext;

    public ProductService(TenderhackDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async IAsyncEnumerable<Product> GetItemsAsync(
      ProductFilter filter, Sorting<ProductSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = Query(_dbContext.Products);

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

    public async Task<int> GetCountAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Products.AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Product?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await Query(_dbContext.Products)
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public void AddItem(Product item)
    {
      _dbContext.Properties.AddRange(item.Properties);
      _dbContext.Products.Add(item);
    }

    public void UpdateItem(Product dbItem, Product item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);

      dbItem.Properties.Clear();
      foreach (var tag in item.Properties)
      {
        dbItem.Properties.Add(tag);
      }
    }

    public async Task<Product> SaveItemAsync(Product item, CancellationToken cancellationToken = default)
    {
      Product? dbItem = null;

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
      return await _dbContext.Products
        .AnyAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);
    }

    public async Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = new Product { Id = id };

      _dbContext.Products.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private static IQueryable<Product> ApplyFilter(IQueryable<Product> query, ProductFilter filter)
    {
      if (filter.Ids != null && filter.Ids.Count != 0)
      {
        var ids = filter.Ids;
        query = query.Where(p => ids.Contains(p.Id));
      }

      if (filter.ExternalIds != null && filter.ExternalIds.Count != 0)
      {
        var externalIds = filter.ExternalIds;
        query = query.Where(p => externalIds.Contains(p.ExternalId));
      }

      if (filter.CategoryIds != null && filter.CategoryIds.Count != 0)
      {
        var categoryIds = filter.CategoryIds;
        query = query.Where(p => categoryIds.Contains(p.CategoryId));
      }

      var q = filter.Q?.Trim().ToLower();
      if (q?.Length >= 3)
      {
        var like = $"%{q}%";
        query = query.Where(p => EF.Functions.ILike(p.Name, like)); // p.Name + " " + p.Property
      }

      return query;
    }

    private static IQueryable<Product> ApplySoring(IQueryable<Product> query, Sorting<ProductSortType> sorting)
    {
      query = sorting.Sort switch
      {
        ProductSortType.Id => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Id),
          _ => query.OrderByDescending(p => p.Id)
        },
        ProductSortType.Name => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Name),
          _ => query.OrderByDescending(p => p.Name)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<Product> ApplyPaging(IQueryable<Product> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }

    private static IQueryable<Product> Query(IQueryable<Product> query)
    {
      return query
        .Include(p => p.Category)
        .Include(p => p.Properties
          .OrderBy(p => p.Attribute.Name)
          .ThenBy(p => p.Value.Name)
        )
        .ThenInclude(p => p.Attribute)
        .Include(p => p.Properties)
        .ThenInclude(p => p.Value)
        .AsSplitQuery()
        .AsNoTracking();
    }
  }
}
