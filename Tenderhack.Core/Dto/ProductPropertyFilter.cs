using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class PropertyFilter
  {
    [MinLength(3), MaxLength(63)]
    public string? Q { get; set; }
    public List<int>? ProductIds { get; set; }
    public List<int>? AttributeIds { get; set; }
    public List<int>? ValueIds { get; set; }
  }
}
