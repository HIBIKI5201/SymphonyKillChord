using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     使用武器種類数の評価条件を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class WeaponVarietyEvaluationConditionAsset : MissionEvaluationConditionAssetBase
    {
        /// <summary>
        ///     使用武器種類数の評価条件を生成します。
        /// </summary>
        /// <returns> 生成した評価条件です。 </returns>
        public override IMissionEvaluationCondition Create()
        {
            return new WeaponVarietyEvaluationCondition(
                EvaluationId,
                _requiredVariety,
                GetDisplayText());
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"{_requiredVariety}種類以上の武器を使用する条件";
        }

        [SerializeField, Min(1), Tooltip("達成に必要な武器種類数です。")]
        private int _requiredVariety = 1;
    }
}
