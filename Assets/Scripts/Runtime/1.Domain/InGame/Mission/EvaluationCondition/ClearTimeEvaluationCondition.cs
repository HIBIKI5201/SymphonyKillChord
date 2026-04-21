namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     一定時間内にクリアすると評価が上がる条件を表すクラス。
    /// </summary>
    public class ClearTimeEvaluationCondition : IMissionEvaluationCondition
    {
        public ClearTimeEvaluationCondition(float thresholdTime)
        {
            _thresholdTime = thresholdTime;
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ElapsedTime.Value <= _thresholdTime;
        }

        public string GetDescription()
        {
            return $"{_thresholdTime}秒以内にクリアで評価アップ";
        }

        private readonly float _thresholdTime;
    }
}
