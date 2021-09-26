using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
  public class ContractFilter
  {
    [MinLength(3), MaxLength(63)]
    public string? Q { get; set; }

    public DateTime? MinPublicAt { get; set; }
    public DateTime? MaxPublicAt { get; set; }

    public DateTime? MinConclusionAt { get; set; }
    public DateTime? MaxConclusionAt { get; set; }

    [Range(0.01, int.MaxValue)]
    public decimal? MinPrice { get; set; }
    [Range(0.01, int.MaxValue)]
    public decimal? MaxPrice { get; set; }

    public List<int>? CustomerIds { get; set; }

    public List<int>? ProviderIds { get; set; }

    public List<int>? Ids { get; set; }
  }
}
