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
  public class PropertyService
  {
    private readonly TenderhackDbContext _dbContext;

    public PropertyService(TenderhackDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async IAsyncEnumerable<ProductProperty> GetItemsAsync(
      PropertyFilter filter, Sorting<PropertySortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = Query(_dbContext.Properties);

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

    public async Task<int> GetCountAsync(PropertyFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Properties.AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public void AddItem(ProductProperty item)
    {
      _dbContext.Properties.Add(item);
    }

    public void UpdateItem(ProductProperty dbItem, ProductProperty item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);
    }

    private static IQueryable<ProductProperty> ApplyFilter(IQueryable<ProductProperty> query, PropertyFilter filter)
    {
      if (filter.ProductIds != null && filter.ProductIds.Count != 0)
      {
        var productIds = filter.ProductIds;
        query = query.Where(p => productIds.Contains(p.ProductId));
      }

      if (filter.AttributeIds != null && filter.AttributeIds.Count != 0)
      {
        var attributeIds = filter.AttributeIds;
        query = query.Where(p => attributeIds.Contains(p.AttributeId));
      }

      if (filter.ValueIds != null && filter.ValueIds.Count != 0)
      {
        var valueIds = filter.ValueIds;
        query = query.Where(p => valueIds.Contains(p.ValueId));
      }

      var q = filter.Q?.Trim().ToLower();
      if (q?.Length >= 3)
      {
        var like = $"%{q}%";
        query = query.Where(p => EF.Functions.ILike(p.Attribute.Name + " " + p.Value.Name, like));
      }

      return query;
    }

    private static IQueryable<ProductProperty> ApplySoring(IQueryable<ProductProperty> query, Sorting<PropertySortType> sorting)
    {
      query = sorting.Sort switch
      {
        PropertySortType.NameValue => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Attribute.Name).ThenBy(p => p.Value.Name),
          _ => query.OrderByDescending(p => p.Attribute.Name).ThenBy(p => p.Value.Name)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<ProductProperty> ApplyPaging(IQueryable<ProductProperty> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }

    private static IQueryable<ProductProperty> Query(IQueryable<ProductProperty> query)
    {
      return query.AsNoTracking();
    }
  }
}
