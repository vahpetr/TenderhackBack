using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class CharacteristicFilter
  {
    [MinLength(3), MaxLength(63)]
    public string? Q { get; set; }

    public List<int>? Ids { get; set; }
  }
}
