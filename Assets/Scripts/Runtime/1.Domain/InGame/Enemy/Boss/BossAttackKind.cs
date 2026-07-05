using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     ボスの攻撃種別。
    /// </summary>
    public enum BossAttackKind
    {
        /// <summary> 直線・射線判定（通常攻撃1）。 </summary>
        Infantry,
        /// <summary> 迫撃・円形範囲（通常攻撃2）。 </summary>
        Artillery,
        /// <summary> 三方向直線（特殊攻撃1）。 </summary>
        TripleShot
    }
}
