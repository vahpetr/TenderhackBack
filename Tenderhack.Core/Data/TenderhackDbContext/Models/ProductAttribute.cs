using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductAttribute
    {
        public int Id { get; set; }
        [Required, MaxLength(511)]
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
        [JsonIgnore]
        public ICollection<ProductProperty> Properties { get; set; }
        [JsonIgnore]
        public ICollection<ProductsAttributes> ProductsAttributes { get; set; }
    }
}
