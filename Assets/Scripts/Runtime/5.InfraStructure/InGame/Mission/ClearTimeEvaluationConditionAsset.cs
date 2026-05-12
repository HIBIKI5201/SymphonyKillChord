using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     クリアタイム評価条件のアセットクラス。
    /// </summary>
    [Serializable]
    public class ClearTimeEvaluationConditionAsset : MissionEvaluationConditionAssetBase
    {
        /// <summary>
        ///     評価条件を生成します。
        /// </summary>
        /// <returns>評価条件。</returns>
        public override IMissionEvaluationCondition Create()
        {
            return new ClearTimeEvaluationCondition(_clearTime, GetDisplayText());
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"クリアタイムが{_clearTime}秒以下である条件";
        }

        [SerializeField, Tooltip("この秒数以内にクリアすると条件達成となります。")] private float _clearTime;
    }
}
