using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     クリティカル率を加算するステータスボーナス効果。
    /// </summary>
    [Serializable]
    public sealed class CriticalChanceAdditionEffect : IStatusBonusEffect
    {
        /// <summary> 既定値で効果を初期化する。 </summary>
        public CriticalChanceAdditionEffect()
        {
        }

        /// <summary>
        ///     指定した加算値で効果を初期化する。
        /// </summary>
        /// <param name="value"> クリティカル率の加算値。 </param>
        public CriticalChanceAdditionEffect(float value)
        {
            _value = value;
        }

        /// <summary> この効果の種別。 </summary>
        public StatusBonusEffectKind Kind => StatusBonusEffectKind.CriticalChance;

        /// <summary>
        ///     クリティカル率の加算値をボーナスへ適用する。
        /// </summary>
        public void Apply(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddCriticalChance(_value);
        }

        [SerializeField, Tooltip("クリティカル率の加算値。0.02は2%を表す。")]
        private float _value = 0.02f;
    }
}
