using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵の照準の始点が、敵自身の位置と同じかを判定する。
    /// </summary>
    public static class EnemyAimOriginUtility
    {
        /// <summary>
        ///     始点が敵自身の位置とみなせるかを返す。
        /// </summary>
        /// <param name="sourcePosition"> 照準の始点。 </param>
        /// <param name="enemyPosition"> 敵自身の位置。 </param>
        /// <returns> 同じ位置とみなせる場合は true。 </returns>
        public static bool IsEnemyOrigin(Vector3 sourcePosition, Vector3 enemyPosition)
        {
            return (sourcePosition - enemyPosition).sqrMagnitude <= SAME_POSITION_SQR_THRESHOLD;
        }

        /// <summary> 同じ位置とみなす距離の二乗の上限。 </summary>
        private const float SAME_POSITION_SQR_THRESHOLD = 0.0001f;
    }
}
