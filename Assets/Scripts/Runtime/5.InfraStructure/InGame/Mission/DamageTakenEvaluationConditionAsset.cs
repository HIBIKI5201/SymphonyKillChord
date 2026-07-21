using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     被ダメージ評価条件を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class DamageTakenEvaluationConditionAsset : MissionEvaluationConditionAssetBase
    {
        /// <summary>
        ///     被ダメージ評価条件を生成します。
        /// </summary>
        /// <returns> 生成した評価条件です。 </returns>
        public override IMissionEvaluationCondition Create()
        {
            return new DamageTakenEvaluationCondition(
                EvaluationId,
                _maximumDamage,
                GetDisplayText());
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"累計被ダメージが{_maximumDamage}以下である条件";
        }

        [SerializeField, Min(0f), Tooltip("達成条件となる累計被ダメージの上限です。")]
        private float _maximumDamage;
    }
}
