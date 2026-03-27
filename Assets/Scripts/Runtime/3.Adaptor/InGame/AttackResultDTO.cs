using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     攻撃処理の結果を表すデータ転送オブジェクト（DTO）。
    /// </summary>
    public readonly struct AttackResultDTO
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="isCritical"></param>
        public AttackResultDTO(float damage, bool isCritical)
        {
            Damage = damage;
            IsCritical = isCritical;
        }

        public float Damage { get; }
        public bool IsCritical { get; }
    }
}
