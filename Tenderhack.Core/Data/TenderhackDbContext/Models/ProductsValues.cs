namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductsValues
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ValueId { get; set; }
        public ProductValue Value { get; set; }
    }
}
