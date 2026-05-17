using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     評価条件を実行して、ミッションの評価結果を生成するクラス。
    /// </summary>
    public class MissionEvaluationRunner
    {
        /// <summary>
        ///     評価を実行します。
        /// </summary>
        /// <param name="progress">進行状況。</param>
        /// <param name="evaluationConditions">評価条件のリスト。</param>
        /// <returns>評価結果。</returns>
        public MissionEvaluationResult Run(
            MissionProgress progress,
            IReadOnlyList<IMissionEvaluationCondition> evaluationConditions)
        {
            if (evaluationConditions == null || evaluationConditions.Count == 0)
            {
                Debug.LogWarning("評価条件が設定されていません。");
                return new MissionEvaluationResult(Array.Empty<MissionEvaluationProgress>());
            }

            MissionEvaluationProgress[] progresses =
                new MissionEvaluationProgress[evaluationConditions.Count];

            for (int i = 0; i < evaluationConditions.Count; i++)
            {
                IMissionEvaluationCondition condition = evaluationConditions[i];
                bool isAchieved = condition != null && condition.IsSatisfied(progress);
                string description = condition != null ? condition.GetDescription() : string.Empty;

                progresses[i] = new MissionEvaluationProgress(description, isAchieved);
            }

            return new MissionEvaluationResult(progresses);
        }
    }
}
