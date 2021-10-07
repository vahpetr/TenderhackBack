using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Data.TenderhackDbContext.Models;
using Tenderhack.Core.Dto;
using Tenderhack.Core.Services;
using Tenderhack.Core.Types;

namespace Tenderhack.Api.Controllers
{
  /// <summary>
  /// Products resource
  /// </summary>
  [ApiController]
  [Route("/api/v1/products")]
  [Produces(MediaTypeNames.Application.Json)]
  [Consumes(MediaTypeNames.Application.Json)]
  public class ProductsController : ControllerBase
  {
    private readonly Lazy<ProductService> _service;
    private readonly Lazy<ILogger<ProductsController>> _logger;

    /// <summary>
    /// Controller constructor
    /// </summary>
    /// <param name="service">Service</param>
    /// <param name="logger">Logger</param>
    /// <exception cref="ArgumentNullException">Not all parameters initialized</exception>
    public ProductsController(
      Lazy<ProductService> service,
      Lazy<ILogger<ProductsController>> logger
      )
    {
      _service = service ?? throw new ArgumentNullException(nameof(service));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get items
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <param name="paging">Paging</param>
    /// <param name="sorting">Sorting</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Items</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesDefaultResponseType]
    [ResponseCache(CacheProfileName = "SharedCache", VaryByQueryKeys = new []{ "*" })]
    public async Task<ActionResult<IAsyncEnumerable<Product>>> GetItemsAsync(
      [FromQuery] ProductFilter filter, [FromQuery] Sorting<ProductSortType> sorting, [FromQuery] Paging paging,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
      )
    {
      var items = _service.Value.GetItemsAsync(filter, sorting, paging, cancellationToken);
      return Ok(items);
    }

    /// <summary>
    /// Get count
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Count</returns>
    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesDefaultResponseType]
    [ResponseCache(CacheProfileName = "SharedCache", VaryByQueryKeys = new []{ "*" })]
    public async Task<ActionResult<int>> GetCountAsync([FromQuery] ProductFilter filter, CancellationToken cancellationToken = default)
    {
      var count = await _service.Value.GetCountAsync(filter, cancellationToken)
        .ConfigureAwait(false);

      return Ok(count);
    }

    /// <summary>
    /// Get item by id
    /// </summary>
    /// <param name="id">Item id</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Item</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ResponseCache(CacheProfileName = "SharedCache", VaryByQueryKeys = new []{ "*" })]
    public async Task<ActionResult<Product>> GetItemAsync(int id, CancellationToken cancellationToken = default)
    {
      var result = await _service.Value.GetItemAsync(id, cancellationToken)
        .ConfigureAwait(false);

      if (result == null) return NotFound();

      return Ok(result);
    }

    // See https://docs.microsoft.com/ru-ru/ef/core/saving/disconnected-entities#handling-deletes

    /// <summary>
    /// Save item
    /// </summary>
    /// <param name="item">Item to save</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Status code</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Product>> SaveItemAsync([FromBody] Product item, CancellationToken cancellationToken = default)
    {
      try
      {
        var result = await _service.Value.SaveItemAsync(item, cancellationToken)
          .ConfigureAwait(false);

        return Ok(result);
      }
      // https://docs.microsoft.com/ru-ru/ef/core/saving/concurrency
      catch (DbUpdateConcurrencyException)
      {
        return Conflict();
      }
      catch (DbUpdateException)
      {
        return BadRequest();
      }
    }

    /// <summary>
    /// Delete item by id
    /// </summary>
    /// <param name="id">Item id</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Status code</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken = default)
    {
      try
      {
        await _service.Value.DeleteItemAsync(id, cancellationToken).ConfigureAwait(false);

        return NoContent();
      }
      catch (DbUpdateConcurrencyException)
      {
        // TODO rewrite
        if (!await _service.Value.ExistAsync(id, cancellationToken))
          return NotFound();

        return Conflict();
      }
      catch (DbUpdateException)
      {
        return BadRequest();
      }
    }
  }
}
