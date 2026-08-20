using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Buff
{
    /// <summary>
    ///     与えるダメージを一定倍率へ変更するバフのアセットです。
    /// </summary>
    [Serializable]
    public sealed class AttackPowerMultiplierBuffAsset : PlayerBuffDefinitionAssetBase
    {
        /// <inheritdoc />
        public override IStatusEffect Create()
        {
            return new AttackPowerMultiplierBuff(_multiplier, ReapplyPolicy);
        }

        [SerializeField, Range(0f, 3f), Tooltip("ダメージ倍率。0で攻撃力無効化相当。")]
        private float _multiplier = 1f;
    }
}
