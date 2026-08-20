using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     最大体力へ倍率加算値を適用するステータスボーナス効果。
    /// </summary>
    [Serializable]
    public sealed class MaxHealthMultiplierEffect : IStatusBonusEffect
    {
        /// <summary> 既定値で効果を初期化する。 </summary>
        public MaxHealthMultiplierEffect()
        {
        }

        /// <summary>
        ///     指定した倍率加算値で効果を初期化する。
        /// </summary>
        /// <param name="multiplier"> 最大体力の倍率加算値。 </param>
        public MaxHealthMultiplierEffect(float multiplier)
        {
            _multiplier = multiplier;
        }

        /// <summary>
        ///     最大体力の倍率加算値をボーナスへ適用する。
        /// </summary>
        public void Apply(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddMaxHealthMultiplier(_multiplier);
        }

        [SerializeField, Tooltip("最大体力に適用する倍率加算値。")]
        private float _multiplier = 0.0625f;
    }
}
