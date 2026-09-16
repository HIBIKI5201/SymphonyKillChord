namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     最終Waveまでのすべての敵を撃破したときに成立する条件です。
    /// </summary>
    public sealed class AllEnemyWavesClearCondition : IMissionClearCondition
    {
        /// <summary>
        ///     すべての敵Waveの撃破完了が記録されているかを判定します。
        /// </summary>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.HasClearedAllEnemyWaves;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        public string GetDescription()
        {
            return "すべてのWaveの敵を撃破する。";
        }
    }
}
