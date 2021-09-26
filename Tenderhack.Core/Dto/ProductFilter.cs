using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class ProductFilter
  {
    [MinLength(3), MaxLength(63)]
    public string? Q { get; set; }
    public List<int>? Ids { get; set; }
    public List<int>? ExternalIds { get; set; }
    public List<int>? CategoryIds { get; set; }
    // public string? CpgzCode { get; set; }
  }
}
