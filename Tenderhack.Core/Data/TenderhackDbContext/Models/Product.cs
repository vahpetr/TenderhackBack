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

        public ICollection<ProductProperty> Properties { get; set; }

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public ICollection<ProductAttribute> Attributes { get; set; }
        [JsonIgnore]
        public ICollection<ProductsAttributes> ProductsAttributes { get; set; }

        [JsonIgnore]
        public ICollection<ProductValue> Values { get; set; }
        [JsonIgnore]
        public ICollection<ProductsValues> ProductsValues { get; set; }
    }
}
