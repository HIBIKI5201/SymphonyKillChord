using System;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     砲弾固有の攻撃パラメータを保持する値オブジェクト。
    /// </summary>
    public readonly struct ShellAttackSpec
    {
        /// <summary>
        ///     爆発半径を指定して生成する。
        ///     負の値は例外を投げる。
        /// </summary>
        public ShellAttackSpec(float explosionRadius)
        {
            if (explosionRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(explosionRadius), "爆発半径の値は0より小さい。");
            }
            ExplosionRadius = explosionRadius;
        }
        /// <summary> 爆発半径 </summary>
        public readonly float ExplosionRadius { get; }
    }
}
