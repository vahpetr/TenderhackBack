using System.ComponentModel;
using Tenderhack.Core.Types;

namespace Tenderhack.Core.Dto
{
    /// <summary>
    /// Sorting
    /// </summary>
    public class Sorting<TSort> where TSort: struct
    {
        /// <summary>
        /// Sort property
        /// </summary>
        public TSort Sort { get; set; } = default(TSort);

        /// <summary>
        /// Sort direction
        /// </summary>
        [DefaultValue(DirectionType.Desc)]
        public DirectionType Dir { get; set; } = DirectionType.Desc;
    }

}
