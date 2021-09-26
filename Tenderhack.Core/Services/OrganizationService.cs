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
  public class OrganizationService
  {
    private readonly TenderhackDbContext _dbContext;

    public OrganizationService(TenderhackDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async IAsyncEnumerable<Organization> GetItemsAsync(
      OrganizationFilter filter, Sorting<OrganizationSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = _dbContext.Organizations
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

    public async Task<int> GetCountAsync(OrganizationFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Organizations
        .AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Organization?> FindItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Organizations
        .FindAsync(new object[] { id }, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public async Task<Organization?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Organizations
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public void AddItem(Organization item)
    {
      _dbContext.Organizations.Add(item);
    }

    public void UpdateItem(Organization dbItem, Organization item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);
    }

    public async Task<Organization> SaveItemAsync(Organization item, CancellationToken cancellationToken = default)
    {
      Organization? dbItem = null;

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
      var item = new Organization() { Id = id };

      _dbContext.Organizations.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private IQueryable<Organization> ApplyFilter(IQueryable<Organization> query, OrganizationFilter filter)
    {
      if (filter.Ids != null && filter.Ids.Count != 0)
      {
        var ids = filter.Ids;
        query = query.Where(p => ids.Contains(p.Id));
      }

      if (filter.Inns != null && filter.Inns.Count != 0)
      {
        var inns = filter.Inns;
        query = query.Where(p => inns.Contains(p.Inn));
      }

      if (filter.Kpps != null && filter.Kpps.Count != 0)
      {
        var kpps = filter.Kpps;
        query = query.Where(p => kpps.Contains(p.Kpp));
      }

      var q = filter.Q?.Trim().ToLower();
      if (q?.Length >= 3)
      {
        var like = $"%{q}%";
        query = query.Where(p => EF.Functions.ILike(p.Name + " " + p.Inn + " " + p.Kpp, like));
      }

      return query;
    }

    private static IQueryable<Organization> ApplySoring(IQueryable<Organization> query, Sorting<OrganizationSortType> sorting)
    {
      query = sorting.Sort switch
      {
        OrganizationSortType.Id => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Id),
          _ => query.OrderByDescending(p => p.Id)
        },
        OrganizationSortType.Inn => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Inn),
          _ => query.OrderByDescending(p => p.Inn)
        },
        OrganizationSortType.Kpp => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Kpp),
          _ => query.OrderByDescending(p => p.Kpp)
        },
        OrganizationSortType.Name => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Name),
          _ => query.OrderByDescending(p => p.Name)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<Organization> ApplyPaging(IQueryable<Organization> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }
  }
}
