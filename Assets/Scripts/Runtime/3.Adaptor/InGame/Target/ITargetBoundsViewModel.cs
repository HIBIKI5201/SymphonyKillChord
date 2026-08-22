using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     ターゲットのワールド空間Boundsを公開するViewModelインターフェース。
    /// </summary>
    public interface ITargetBoundsViewModel
    {
        /// <summary> ターゲットのワールド空間Bounds。 </summary>
        Bounds WorldBounds { get; }
    }
}
