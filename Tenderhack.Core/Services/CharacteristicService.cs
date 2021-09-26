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
  public class CharacteristicService
  {
    private readonly TenderhackDbContext _dbContext;

    public CharacteristicService(TenderhackDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async IAsyncEnumerable<Characteristic> GetItemsAsync(
      CharacteristicFilter filter, Sorting<CharacteristicSortType> sorting, Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var query = _dbContext.Characteristics
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

    public async Task<int> GetCountAsync(CharacteristicFilter filter, CancellationToken cancellationToken = default)
    {
      var query = _dbContext.Characteristics
        .AsNoTracking();

      query = ApplyFilter(query, filter);

      var count = await query
        .CountAsync(cancellationToken)
        .ConfigureAwait(false);

      return count;
    }

    public async Task<Characteristic?> FindItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Characteristics
        .FindAsync(new object[] { id }, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public async Task<Characteristic?> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var item = await _dbContext.Characteristics
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        .ConfigureAwait(false);

      return item;
    }

    public void AddItem(Characteristic item)
    {
      _dbContext.Characteristics.Add(item);
    }

    public void UpdateItem(Characteristic dbItem, Characteristic item)
    {
      _dbContext.Entry(dbItem).CurrentValues.SetValues(item);
    }

    public async Task<Characteristic> SaveItemAsync(Characteristic item, CancellationToken cancellationToken = default)
    {
      Characteristic? dbItem = null;

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
      var item = new Characteristic { Id = id };

      _dbContext.Characteristics.Remove(item);

      await _dbContext
        .SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);
    }

    private IQueryable<Characteristic> ApplyFilter(IQueryable<Characteristic> query, CharacteristicFilter filter)
    {
      if (filter.Ids != null && filter.Ids.Count != 0)
      {
        var ids = filter.Ids;
        query = query.Where(p => ids.Contains(p.Id));
      }

      var q = filter.Q?.Trim().ToLower();
      if (q?.Length >= 3)
      {
        var like = $"%{q}%";
        query = query.Where(p => EF.Functions.ILike(p.Name, like));
      }

      return query;
    }

    private static IQueryable<Characteristic> ApplySoring(IQueryable<Characteristic> query, Sorting<CharacteristicSortType> sorting)
    {
      query = sorting.Sort switch
      {
        CharacteristicSortType.Id => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Id),
          _ => query.OrderByDescending(p => p.Id)
        },
        CharacteristicSortType.Name => sorting.Dir switch
        {
          DirectionType.Asc => query.OrderBy(p => p.Name),
          _ => query.OrderByDescending(p => p.Name)
        },
        _ => throw new ArgumentException(nameof(sorting.Sort))
      };

      return query;
    }

    private static IQueryable<Characteristic> ApplyPaging(IQueryable<Characteristic> query, Paging paging)
    {
      return query.Skip(paging.Skip).Take(paging.Take);
    }
  }
}
