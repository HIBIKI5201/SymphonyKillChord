using System;

namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     累計被ダメージがしきい値以下であることを評価します。
    /// </summary>
    public sealed class DamageTakenEvaluationCondition : IMissionEvaluationCondition
    {
        /// <summary>
        ///     被ダメージ評価条件を生成します。
        /// </summary>
        /// <param name="evaluationId"> 評価条件IDです。 </param>
        /// <param name="maximumDamage"> 許容する累計ダメージです。 </param>
        /// <param name="description"> 表示文です。 </param>
        public DamageTakenEvaluationCondition(
            MissionEvaluationId evaluationId,
            float maximumDamage,
            string description)
        {
            if (maximumDamage < 0f || float.IsNaN(maximumDamage) || float.IsInfinity(maximumDamage))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumDamage));
            }

            MissionEvaluationId = evaluationId;
            _maximumDamage = maximumDamage;
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
            return progress.DamageTaken.Value <= _maximumDamage;
        }

        /// <summary>
        ///     条件の失敗が確定しているか判定します。
        /// </summary>
        /// <param name="progress"> ミッション進行です。 </param>
        /// <returns> 失敗が確定している場合はtrueです。 </returns>
        public bool IsFailed(MissionProgress progress)
        {
            return progress.DamageTaken.Value > _maximumDamage;
        }

        /// <summary>
        ///     条件の説明文を返します。
        /// </summary>
        /// <returns> 説明文です。 </returns>
        public string GetDescription()
        {
            return _description;
        }

        private readonly float _maximumDamage;
        private readonly string _description;
    }
}
