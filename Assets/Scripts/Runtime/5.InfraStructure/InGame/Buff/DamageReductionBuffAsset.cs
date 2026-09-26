using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Buff
{
    /// <summary>
    ///     被ダメージを一定割合軽減するバフのアセットです。
    /// </summary>
    [Serializable]
    public sealed class DamageReductionBuffAsset : PlayerBuffDefinitionAssetBase
    {
        /// <inheritdoc />
        public override IStatusEffect Create()
        {
            return new DamageReductionBuff(_reductionRate, ReapplyPolicy);
        }

        [SerializeField, Range(0f, 1f), Tooltip("被ダメージ軽減割合。1で被ダメージ0(無敵相当)。")]
        private float _reductionRate = 1f;
    }
}
