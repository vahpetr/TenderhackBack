using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tenderhack.Core.Dto
{
    /// <summary>
    /// Paging
    /// </summary>
    public class Paging
    {
        // TODO change to long

        /// <summary>
        /// Item per range
        /// </summary>
        [Range(1, 1000), DefaultValue(100)]
        public int Take { get; set; } = 100;

        // TODO change to long

        /// <summary>
        /// Skip item count
        /// </summary>
        [Range(0, int.MaxValue), DefaultValue(0)]
        public int Skip { get; set; }
    }

}
