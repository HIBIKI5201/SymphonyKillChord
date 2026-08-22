using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Buff
{
    /// <summary>
    ///     プレイヤーへ付与するバフ・デバフ定義アセットの基底クラス。
    /// </summary>
    [Serializable]
    public abstract class PlayerBuffDefinitionAssetBase
    {
        /// <summary>
        ///     状態効果を生成する。
        /// </summary>
        /// <returns> 生成された状態効果。 </returns>
        public abstract IStatusEffect Create();

        /// <summary>
        ///     同じ状態効果が再適用されたときの挙動を取得します。
        /// </summary>
        protected StatusEffectReapplyPolicy ReapplyPolicy => _reapplyPolicy;

        [SerializeField, Tooltip("同じ状態効果が再適用されたときの挙動")]
        private StatusEffectReapplyPolicy _reapplyPolicy = StatusEffectReapplyPolicy.Ignore;
    }
}
