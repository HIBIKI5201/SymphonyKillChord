using KillChord.Runtime.Adaptor.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Battle
{
    /// <summary>
    ///     攻撃の原点と対象の間に障害物があるかを、線分のレイキャストで判定するクラス。
    /// </summary>
    public sealed class AttackOcclusionView : IAttackOcclusionViewModel
    {
        /// <summary>
        ///     判定クラスを生成する。
        /// </summary>
        /// <param name="obstacleLayers"> 攻撃を遮る障害物のレイヤーです。 </param>
        /// <param name="rayHeight"> 原点と対象の足元から、レイを通す高さ（メートル）です。 </param>
        public AttackOcclusionView(LayerMask obstacleLayers, float rayHeight)
        {
            _obstacleLayers = obstacleLayers;
            _rayHeight = rayHeight;
        }

        /// <summary>
        ///     攻撃の原点から対象までの間に障害物があるかを判定する。
        /// </summary>
        /// <param name="origin"> 攻撃の原点です。 </param>
        /// <param name="targetPosition"> 対象の座標です。 </param>
        /// <returns> 障害物に遮られている場合はtrue。 </returns>
        public bool IsOccluded(in Vector3 origin, in Vector3 targetPosition)
        {
            // 座標は足元のため、地面の起伏や低い段差に当たらないよう高さを持ち上げてから線分を引く。
            Vector3 offset = Vector3.up * _rayHeight;
            return Physics.Linecast(
                origin + offset,
                targetPosition + offset,
                _obstacleLayers,
                QueryTriggerInteraction.Ignore);
        }

        private readonly LayerMask _obstacleLayers;
        private readonly float _rayHeight;
    }
}
