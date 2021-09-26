using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tenderhack.Core.Dto;
using Tenderhack.Core.Services;

namespace Tenderhack.Api.Controllers
{
  /// <summary>
  /// Orders resource
  /// </summary>
  [ApiController]
  [Route("/api/v1/orders")]
  [Produces(MediaTypeNames.Application.Json)]
  [Consumes(MediaTypeNames.Application.Json)]
  public class OrdersController : ControllerBase
  {
    private readonly Lazy<PredictService> _service;
    private readonly Lazy<ILogger<OrdersController>> _logger;

    /// <summary>
    /// Controller constructor
    /// </summary>
    /// <param name="service">Service</param>
    /// <param name="logger">Logger</param>
    /// <exception cref="ArgumentNullException">Not all parameters initialized</exception>
    public OrdersController(
      Lazy<PredictService> service,
      Lazy<ILogger<OrdersController>> logger
      )
    {
      _service = service ?? throw new ArgumentNullException(nameof(service));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Predict demands
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <param name="paging">Paging</param>
    /// <param name="sorting">Sorting</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Items</returns>
    [HttpGet("predict/demands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ResponseCache(CacheProfileName = "Caching", VaryByQueryKeys = new []{ "*" })]
    public async IAsyncEnumerable<ScoredOrder> PredictDemandsAsync(
      [FromQuery] PredictDemandsRequest request,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
      )
    {
      var items = _service.Value
        .PredictDemandsAsync(request, cancellationToken)
        .ConfigureAwait(false);

      await foreach (var item in items)
      {
        yield return item;

        try
        {
          cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
          Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
          yield break;
        }
      }
    }

    /// <summary>
    /// Predict purchases
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <param name="paging">Paging</param>
    /// <param name="sorting">Sorting</param>
    /// <param name="cancellationToken">Request cancellation token</param>
    /// <returns>Items</returns>
    [HttpGet("predict/purchases")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ResponseCache(CacheProfileName = "Caching", VaryByQueryKeys = new []{ "*" })]
    public async IAsyncEnumerable<ScoredOrder> PredictPurchasesAsync(
      [FromQuery] PredictPurchasesRequest request,
      [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
      var items = _service.Value
        .PredictPurchasesAsync(request, cancellationToken)
        .ConfigureAwait(false);

      await foreach (var item in items)
      {
        yield return item;

        try
        {
          cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
          Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
          yield break;
        }
      }
    }
  }
}
