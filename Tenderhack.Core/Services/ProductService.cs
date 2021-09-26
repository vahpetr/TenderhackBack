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
    private readonly CharacteristicService _characteristicService;

    public ProductService(TenderhackDbContext dbContext, CharacteristicService characteristicService)
    {
      _dbContext = dbContext;
      _characteristicService = characteristicService;
    }

    public async IAsyncEnumerable<Product> GetItemsAsync(
      ProductFilter filter, Sorting<ProductSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = _dbContext.Products
        .Include(p => p.Category)
        .Include(p => p.Characteristics.OrderBy(t => t.Name))
        .AsSplitQuery()
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

    public async Task<int> GetCountAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Products
        .AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Product?> FindItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Products
        .FindAsync(new object[] { id }, cancellationToken)
        .ConfigureAwait(false);

      if (item == null)
      {
        return null;
      }

      await _dbContext.Entry(item)
        .Collection(p => p.Characteristics.OrderBy(t => t.Name))
        .LoadAsync(cancellationToken)
        .ConfigureAwait(false);

      await _dbContext.Entry(item)
        .Reference(p => p.Category)
        .LoadAsync(cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public async Task<Product?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.Characteristics.OrderBy(t => t.Name))
        .AsSplitQuery()
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      if (item == null)
      {
        return null;
      }

      return item;
    }

    public void AddItem(Product item)
    {
      foreach (var tag in item.Characteristics)
      {
        if (tag.Id == 0)
        {
          _characteristicService.AddItem(tag);
        }
        else
        {
          _dbContext.Entry(tag).State = EntityState.Unchanged;
        }
      }

      _dbContext.Products.Add(item);
    }

    public void UpdateItem(Product dbItem, Product item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);

      var dbTagMap = dbItem.Characteristics.ToDictionary(p => p.Id);
      foreach (var tag in item.Characteristics)
      {
        if (dbTagMap.TryGetValue(tag.Id, out var dbTag))
        {
          // update
          _characteristicService.UpdateItem(dbTag, tag);
        }
        else
        {
          if (tag.Id == 0)
          {
            _characteristicService.AddItem(tag);
          }
          else
          {
            _dbContext.Entry(tag).State = EntityState.Unchanged;
          }

          // attach
          dbItem.Characteristics.Add(tag);
        }
      }

      var tagMap = item.Characteristics.ToDictionary(p => p.Id);
      foreach (var dbTag in dbItem.Characteristics)
      {
        if (!tagMap.ContainsKey(dbTag.Id))
        {
          // detach
          dbItem.Characteristics.Remove(dbTag);
        }
      }
    }

    public async Task<Product> SaveItemAsync(Product item, CancellationToken cancellationToken = default)
    {
      Product? dbItem = null;

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
      var item = new Product { Id = id };

      _dbContext.Products.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private IQueryable<Product> ApplyFilter(IQueryable<Product> query, ProductFilter filter)
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
  }
}
