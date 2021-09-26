using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Tenderhack.PredictQuantity.Model;
using Microsoft.AspNetCore.Http;

namespace Tenderhack.Api.Controllers
{
  [ApiController]
  [Route("/api/v1/predict")]
  public class PredictController : ControllerBase
  {
    private readonly PredictionEnginePool<PredictQuantityInput, PredictQuantityOutput> _predictionEnginePool;

    public PredictController(PredictionEnginePool<PredictQuantityInput, PredictQuantityOutput> predictionEnginePool)
    {
      _predictionEnginePool = predictionEnginePool;
    }

    [HttpPost("quantity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public ActionResult<PredictQuantityOutput> Post([FromBody] PredictQuantityInput input)
    {
      if (!ModelState.IsValid) return BadRequest();

      var prediction = _predictionEnginePool.Predict("PredictQuantity", input);

      return Ok(prediction);
    }
  }
}
