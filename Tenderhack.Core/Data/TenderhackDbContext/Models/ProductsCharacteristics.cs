namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class ProductsCharacteristics
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CharacteristicId { get; set; }
        public Characteristic Characteristic { get; set; }
    }
}
