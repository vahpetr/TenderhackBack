using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenderhack.Core.Data.TenderhackDbContext.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, MaxLength(511)]
        public string Title { get; set; }

        [Required, MaxLength(32)]
        public string Kpgz { get; set; }

        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
