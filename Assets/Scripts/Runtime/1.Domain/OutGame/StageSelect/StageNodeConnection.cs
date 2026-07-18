namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノード間の接続を表す値型オブジェクト。
    /// </summary>
    public readonly struct StageNodeConnection
    {
        /// <summary>
        ///     ステージノード間の接続を初期化する。
        /// </summary>
        /// <param name="fromStageId"> 接続元のステージID。</param>
        /// <param name="toStageId"> 接続先のステージID。</param>
        /// <param name="advanceMode"> 接続元ステージ完了後の進行方法。</param>
        public StageNodeConnection(
            StageId fromStageId,
            StageId toStageId,
            StageAdvanceMode advanceMode)
        {
            _fromStageId = fromStageId;
            _toStageId = toStageId;
            _advanceMode = advanceMode;
        }

        /// <summary> 接続元のステージID。 </summary>
        public StageId FromStageId => _fromStageId;
        /// <summary> 接続先のステージID。 </summary>
        public StageId ToStageId => _toStageId;
        /// <summary> 接続元ステージ完了後の進行方法。 </summary>
        public StageAdvanceMode AdvanceMode => _advanceMode;

        private readonly StageId _fromStageId;
        private readonly StageId _toStageId;
        private readonly StageAdvanceMode _advanceMode;
    }
}
