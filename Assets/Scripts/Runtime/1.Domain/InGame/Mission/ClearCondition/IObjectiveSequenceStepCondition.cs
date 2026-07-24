namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>Objectiveステップ開始時の進行値を基準として評価する条件です。</summary>
    public interface IObjectiveSequenceStepCondition
    {
        /// <summary>ステップ開始時の進行値を記録します。</summary>
        /// <param name="progress">Mission進行状況です。</param>
        void BeginStep(MissionProgress progress);
    }
}
