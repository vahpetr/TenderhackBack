using System.ComponentModel.DataAnnotations;

namespace Tenderhack.ContractLoader
{
  public class RawOrder
  {
    public int? Id { get; set; }
    /// <summary>
    /// Count
    /// </summary>
    [Range(0.00, float.MaxValue)]
    public decimal Quantity { get; set; }
    /// <summary>
    /// Price
    /// </summary>
    [Range(0.01, float.MaxValue)]
    public decimal Amount { get; set; }
  }
}
