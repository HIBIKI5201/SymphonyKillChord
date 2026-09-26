using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Placement
{
    /// <summary>
    ///     エフェクトの向き計算を提供するユーティリティ。
    /// </summary>
    public static class SkillEffectRotationUtility
    {
        /// <summary>
        ///     方向ベクトルから回転を求める。
        /// </summary>
        /// <param name="direction"> 方向ベクトルです。 </param>
        /// <returns> 求めた回転です。方向が無効な場合は無回転を返します。 </returns>
        public static Quaternion FromDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= MINIMUM_SQR_MAGNITUDE)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(direction);
        }

        private const float MINIMUM_SQR_MAGNITUDE = 0.0001f;
    }
}
