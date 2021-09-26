using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Dto
{
  public class ScoredOrder
  {
    public Order Order { get; set; }
    public float Score { get; set; }
  }
}
