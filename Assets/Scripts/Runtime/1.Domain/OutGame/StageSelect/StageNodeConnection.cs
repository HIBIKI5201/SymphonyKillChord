using UnityEngine;

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
        public StageNodeConnection(StageId fromStageId, StageId toStageId)
        {
            _fromStageId = fromStageId;
            _toStageId = toStageId;
        }

        /// <summary> 接続元のステージID。 </summary>
        public StageId FromStageId => _fromStageId;
        /// <summary> 接続先のステージID。 </summary>
        public StageId ToStageId => _toStageId;

        private readonly StageId _fromStageId;
        private readonly StageId _toStageId;
    }
}
