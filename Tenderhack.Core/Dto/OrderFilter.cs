using System.Collections.Generic;

namespace Tenderhack.Core.Dto
{
  public class OrderFilter
  {
    public List<int>? ProductIds { get; set; }
    public List<int>? Ids { get; set; }
    public List<int>? ContractProviderIds { get; set; }
    public List<int>? ContractCustomerIds { get; set; }
  }
}
