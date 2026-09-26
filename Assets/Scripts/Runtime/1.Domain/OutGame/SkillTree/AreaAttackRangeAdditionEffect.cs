using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     範囲攻撃の射程を加算するステータスボーナス効果。
    /// </summary>
    [Serializable]
    public sealed class AreaAttackRangeAdditionEffect : IStatusBonusEffect
    {
        /// <summary> 既定値で効果を初期化する。 </summary>
        public AreaAttackRangeAdditionEffect()
        {
        }

        /// <summary>
        ///     指定した加算値で効果を初期化する。
        /// </summary>
        /// <param name="value"> 範囲攻撃射程の加算値。 </param>
        public AreaAttackRangeAdditionEffect(float value)
        {
            _value = value;
        }

        /// <summary> この効果の種別。 </summary>
        public StatusBonusEffectKind Kind => StatusBonusEffectKind.AreaAttackRange;

        /// <summary>
        ///     範囲攻撃射程の加算値をボーナスへ適用する。
        /// </summary>
        public void Apply(PlayerStatusBonusBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddAreaAttackRange(_value);
        }

        [SerializeField, Tooltip("範囲攻撃の射程に加算する距離。")]
        private float _value = 0.3f;
    }
}
