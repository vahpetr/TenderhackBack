using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Organization
    {
        public int Id { get; set; }
        [Required, MaxLength(511)]
        public string Name { get; set; }
        [Required, MaxLength(12)]
        public string Inn { get; set; }
        [Required, MaxLength(9)]
        public string Kpp { get; set; }

        [JsonIgnore]
        public ICollection<Contract> PurchaseHistory { get; set; } = new List<Contract>();
        [JsonIgnore]
        public ICollection<Contract> SaleHistory { get; set; } = new List<Contract>();
    }
}
