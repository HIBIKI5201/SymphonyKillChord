using System;

namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     一定時間内にクリアすると評価が上がる条件を表すクラス。
    /// </summary>
    public class ClearTimeEvaluationCondition : IMissionEvaluationCondition
    {
        /// <summary>
        ///     ClearTimeEvaluationCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="thresholdTime">しきい値となる時間。</param>
        /// <param name="description">条件の説明文。</param>
        public ClearTimeEvaluationCondition(float thresholdTime, string description)
        {
            if (thresholdTime < 0f || float.IsNaN(thresholdTime) || float.IsInfinity(thresholdTime))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(thresholdTime),
                    "thresholdTime must be non-negative and finite.");
            }

            _thresholdTime = thresholdTime;
            _description = description;
        }

        public MissionEvaluationResultTiming ResultTiming => MissionEvaluationResultTiming.Cleared;

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            // 経過時間がしきい値以下であれば達成。
            return progress.ElapsedTime.Value <= _thresholdTime;
        }

        /// <summary>
        ///     制限時間を超過しているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>制限時間を超過している場合は true、そうでない場合は false。</returns>
        public bool IsFailed(MissionProgress progress)
        {
            return progress.ElapsedTime.Value > _thresholdTime;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return _description;
        }

        /// <summary> しきい値時間。 </summary>
        private readonly float _thresholdTime;
        /// <summary> 説明文。 </summary>
        private readonly string _description;
    }
}
