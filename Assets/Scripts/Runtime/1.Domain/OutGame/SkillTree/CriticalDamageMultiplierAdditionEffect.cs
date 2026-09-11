using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     クリティカルダメージ倍率を加算するステータスボーナス効果。
    /// </summary>
    [Serializable]
    public sealed class CriticalDamageMultiplierAdditionEffect : IStatusBonusEffect
    {
        /// <summary> 既定値で効果を初期化する。 </summary>
        public CriticalDamageMultiplierAdditionEffect()
        {
        }

        /// <summary>
        ///     指定した加算値で効果を初期化する。
        /// </summary>
        /// <param name="value"> クリティカルダメージ倍率の加算値。 </param>
        public CriticalDamageMultiplierAdditionEffect(float value)
        {
            _value = value;
        }

        /// <summary> この効果の種別。 </summary>
        public StatusBonusEffectKind Kind => StatusBonusEffectKind.CriticalDamage;

        /// <summary>
        ///     クリティカルダメージ倍率の加算値をボーナスへ適用する。
        /// </summary>
        public void Apply(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddCriticalMultiplier(_value);
        }

        [SerializeField, Tooltip("クリティカルダメージ倍率の加算値。0.1は10%を表す。")]
        private float _value = 0.1f;
    }
}
