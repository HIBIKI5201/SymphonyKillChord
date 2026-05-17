using System;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     一定時間以上経過すると失敗する条件を表すクラス。
    /// </summary>
    public class ElapsedTimeFailCondition : IMissionFailCondition
    {
        /// <summary>
        ///     ElapsedTimeFailCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="timeLimit">制限時間。</param>
        public ElapsedTimeFailCondition(float timeLimit)
        {
            if (timeLimit <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeLimit),
                    "制限時間は0より大きい値を指定してください。");
            }

            _timeLimit = timeLimit;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return $"{_timeLimit}秒以上経過すると失敗。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ElapsedTime.Value >= _timeLimit;
        }

        /// <summary> 制限時間。 </summary>
        private readonly float _timeLimit;
    }
}
