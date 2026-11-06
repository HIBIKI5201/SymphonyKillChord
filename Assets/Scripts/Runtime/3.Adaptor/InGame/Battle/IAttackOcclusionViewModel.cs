using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     攻撃が障害物に遮られるかを判定するViewModel。
    /// </summary>
    public interface IAttackOcclusionViewModel
    {
        /// <summary>
        ///     攻撃の原点から対象までの間に障害物があるかを判定する。
        /// </summary>
        /// <param name="origin"> 攻撃の原点です。 </param>
        /// <param name="targetPosition"> 対象の座標です。 </param>
        /// <returns> 障害物に遮られている場合はtrue。 </returns>
        public bool IsOccluded(in Vector3 origin, in Vector3 targetPosition);
    }
}
