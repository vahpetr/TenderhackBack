using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Characteristic
    {
        public int Id { get; set; }
        public int ExternalId { get; set; }

        [Required, MaxLength(511)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Value { get; set; }

        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();

        [JsonIgnore]
        public ICollection<ProductsCharacteristics> ProductsCharacteristics { get; set; } =
          new List<ProductsCharacteristics>();
    }
}
