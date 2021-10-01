using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Order
    {
        public int Id { get; set; }
        [JsonIgnore]
        public int ContractId { get; set; }
        [JsonIgnore]
        public Contract Contract { get; set; }
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
        /// <summary>
        /// Count
        /// </summary>
        [Range(0, float.MaxValue)]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [Range(0.01, float.MaxValue)]
        public decimal Amount { get; set; }
    }
}
