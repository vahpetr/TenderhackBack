using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int ExternalId { get; set; }
        [Required, MaxLength(511)]
        public string Name { get; set; }

        [JsonIgnore]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required, MaxLength(32)]
        public string CpgzCode { get; set; }

        public ICollection<Characteristic> Characteristics { get; set; } = new List<Characteristic>();
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        [JsonIgnore]
        public ICollection<ProductsCharacteristics> ProductsCharacteristics { get; set; } =
          new List<ProductsCharacteristics>();
    }
}
