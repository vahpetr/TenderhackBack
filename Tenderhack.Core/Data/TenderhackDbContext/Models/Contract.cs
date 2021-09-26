using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required, MaxLength(511)]
        public string Number { get; set; }

        public DateTime PublicAt { get; set; }
        public DateTime? ConclusionAt { get; set; }

        [Range(0.01, float.MaxValue)]
        public decimal Price { get; set; }

        [JsonIgnore]
        public int CustomerId { get; set; }
        public Organization Customer { get; set; }

        [JsonIgnore]
        public int? ProviderId { get; set; }
        public Organization? Provider { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
