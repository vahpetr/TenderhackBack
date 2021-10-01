using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductValue
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
        [JsonIgnore]
        public ICollection<ProductProperty> Properties { get; set; }
        [JsonIgnore]
        public ICollection<ProductsValues> ProductsValues { get; set; }
    }
}
