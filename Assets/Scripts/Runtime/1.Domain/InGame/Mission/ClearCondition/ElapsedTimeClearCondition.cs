using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     一定時間経過でクリアとなる条件。
    ///     生存ミッションなどで使用されることを想定しています。
    /// </summary>
    public class ElapsedTimeClearCondition : IMissionClearCondition
    {
        public ElapsedTimeClearCondition(float requiredTime)
        {
            if (requiredTime < 0f || float.IsNaN(requiredTime) || float.IsInfinity(requiredTime))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredTime),
                    "requiredTime must be non-negative and finite.");
            }

            _requiredTime = requiredTime;
        }

        public string GetDescription()
        {
            return $"{_requiredTime}秒生存する";
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ElapsedTime.Value >= _requiredTime;
        }

        private readonly float _requiredTime;
    }
}
