using System;

namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     最大コンボ数がしきい値以上であることを評価します。
    /// </summary>
    public sealed class ComboEvaluationCondition : IMissionEvaluationCondition
    {
        /// <summary>
        ///     コンボ評価条件を生成します。
        /// </summary>
        /// <param name="evaluationId"> 評価条件IDです。 </param>
        /// <param name="requiredCombo"> 必要なコンボ数です。 </param>
        /// <param name="description"> 表示文です。 </param>
        public ComboEvaluationCondition(
            MissionEvaluationId evaluationId,
            int requiredCombo,
            string description)
        {
            if (requiredCombo <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredCombo));
            }

            MissionEvaluationId = evaluationId;
            _requiredCombo = requiredCombo;
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
            return progress.MaxCombo.Value >= _requiredCombo;
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

        private readonly int _requiredCombo;
        private readonly string _description;
    }
}
