using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     最大コンボ数の評価条件を入力するAssetです。
    /// </summary>
    [Serializable]
    public sealed class ComboEvaluationConditionAsset : MissionEvaluationConditionAssetBase
    {
        /// <summary>
        ///     最大コンボ数の評価条件を生成します。
        /// </summary>
        /// <returns> 生成した評価条件です。 </returns>
        public override IMissionEvaluationCondition Create()
        {
            return new ComboEvaluationCondition(
                EvaluationId,
                _requiredCombo,
                GetDisplayText());
        }

        /// <summary>
        ///     設定内容のサマリーを生成します。
        /// </summary>
        /// <returns> サマリーです。 </returns>
        protected override string BuildSummary()
        {
            return $"最大コンボ数が{_requiredCombo}以上である条件";
        }

        [SerializeField, Min(1), Tooltip("達成に必要な最大コンボ数です。")]
        private int _requiredCombo = 1;
    }
}
