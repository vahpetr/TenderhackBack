using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductProperty
    {
      [JsonIgnore]
      public int ProductId { get; set; }
      [JsonIgnore]
      public Product Product { get; set; }
      [JsonIgnore]
      public int AttributeId { get; set; }
      public ProductAttribute Attribute { get; set; }
      [JsonIgnore]
      public int ValueId { get; set; }
      public ProductValue Value { get; set; }
    }
}
