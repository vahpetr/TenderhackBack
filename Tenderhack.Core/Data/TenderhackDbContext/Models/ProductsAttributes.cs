namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductsAttributes
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int AttributeId { get; set; }
        public ProductAttribute Attribute { get; set; }
    }
}
