using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     攻撃の対象を表すインターフェース。
    /// </summary>
    public interface IHitTarget
    {
        /// <summary> 体力情報。 </summary>
        HealthEntity Health { get; }
    }
}
