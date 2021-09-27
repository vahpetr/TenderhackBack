using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class PredictDemandsRequest
  {
    [Required]
    public string Inn { get; set; }
    [Required]
    public string Kpp { get; set; }
    [Range(1, 1000), DefaultValue(100)]
    public int Take { get; set; } = 100;
    [Range(int.MinValue, -1), DefaultValue(-1)]
    public int YearOffset { get; set; } = -1;
  }
}
