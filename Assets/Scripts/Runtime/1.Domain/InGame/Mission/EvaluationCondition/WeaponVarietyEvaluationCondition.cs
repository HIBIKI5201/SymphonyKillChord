using System;

namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     使用した武器種類数がしきい値以上であることを評価します。
    /// </summary>
    public sealed class WeaponVarietyEvaluationCondition : IMissionEvaluationCondition
    {
        /// <summary>
        ///     武器種類評価条件を生成します。
        /// </summary>
        /// <param name="evaluationId"> 評価条件IDです。 </param>
        /// <param name="requiredVariety"> 必要な武器種類数です。 </param>
        /// <param name="description"> 表示文です。 </param>
        public WeaponVarietyEvaluationCondition(
            MissionEvaluationId evaluationId,
            int requiredVariety,
            string description)
        {
            if (requiredVariety <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredVariety));
            }

            MissionEvaluationId = evaluationId;
            _requiredVariety = requiredVariety;
            _description = description ?? string.Empty;
        }

        /// <summary> 評価条件IDです。 </summary>
        public MissionEvaluationId MissionEvaluationId { get; }

        /// <summary> クリア時に結果を確定します。 </summary>
        public MissionEvaluationResultTiming ResultTiming => MissionEvaluationResultTiming.Cleared;

        /// <summary>
        ///     条件を満たしているか判定します。
        /// </summary>
        /// <param name="progress"> ミッション進行です。 </param>
        /// <returns> 条件を満たす場合はtrueです。 </returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.WeaponVariety.Value >= _requiredVariety;
        }

        /// <summary>
        ///     条件の失敗が確定しているか判定します。
        /// </summary>
        /// <param name="progress"> ミッション進行です。 </param>
        /// <returns> 失敗が確定している場合はtrueです。 </returns>
        public bool IsFailed(MissionProgress progress)
        {
            return progress.IsFinished && !IsSatisfied(progress);
        }

        /// <summary>
        ///     条件の説明文を返します。
        /// </summary>
        /// <returns> 説明文です。 </returns>
        public string GetDescription()
        {
            return _description;
        }

        private readonly int _requiredVariety;
        private readonly string _description;
    }
}
