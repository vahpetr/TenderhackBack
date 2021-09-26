using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class PredictPurchasesRequest
  {
    [Required]
    public string Inn { get; set; }
    [Required]
    public string Kpp { get; set; }
    [Range(1, 100), DefaultValue(20)]
    public int Take { get; set; } = 20;
    [Range(int.MinValue, -1), DefaultValue(-1)]
    public int YearOffset { get; set; } = -1;

    // public string? CpgzCode { get; set; }
  }
}
