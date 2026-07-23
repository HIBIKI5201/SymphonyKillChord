namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     開始時に次の敵Waveを生成する目標シーケンスのステップです。
    /// </summary>
    public sealed class WaveObjectiveSequenceStep : ObjectiveSequenceStep
    {
        /// <summary>
        ///     WaveObjectiveSequenceStep クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="condition"> このステップの達成条件です。 </param>
        /// <param name="guideMessageText"> ステップ開始時に案内するメッセージです。 </param>
        public WaveObjectiveSequenceStep(
            IMissionClearCondition condition,
            string guideMessageText)
            : base(condition, guideMessageText)
        {
        }
    }
}
