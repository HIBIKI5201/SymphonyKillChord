using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     攻撃力へ倍率加算値を適用するステータスボーナス効果。
    /// </summary>
    [Serializable]
    public sealed class AttackPowerMultiplierEffect : IStatusBonusEffect
    {
        /// <summary> 既定値で効果を初期化する。 </summary>
        public AttackPowerMultiplierEffect()
        {
        }

        /// <summary>
        ///     指定した倍率加算値で効果を初期化する。
        /// </summary>
        /// <param name="multiplier"> 攻撃力の倍率加算値。 </param>
        public AttackPowerMultiplierEffect(float multiplier)
        {
            _multiplier = multiplier;
        }

        /// <summary>
        ///     攻撃力の倍率加算値をボーナスへ適用する。
        /// </summary>
        public void Apply(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddAttackPowerMultiplier(_multiplier);
        }

        [SerializeField, Tooltip("攻撃力に適用する倍率加算値。")]
        private float _multiplier = 0.0625f;
    }
}
