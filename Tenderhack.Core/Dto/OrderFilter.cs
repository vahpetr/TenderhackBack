using System.Collections.Generic;

namespace Tenderhack.Core.Dto
{
  public class OrderFilter
  {
    public List<int>? Ids { get; set; }
    public List<int>? ProductIds { get; set; }
    public List<int>? ContractIds { get; set; }
  }
}
