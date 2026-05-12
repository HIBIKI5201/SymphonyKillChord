using System;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     一定時間以上経過すると失敗する条件を表すクラス。
    /// </summary>
    public class ElapsedTimeFailCondition : IMissionFailCondition
    {
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

        public string GetDescription()
        {
            return $"{_timeLimit}秒以上経過すると失敗";
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ElapsedTime.Value >= _timeLimit;
        }

        private readonly float _timeLimit;
    }
}
