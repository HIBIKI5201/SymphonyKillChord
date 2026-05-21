using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ選択画面のステージノードを表すクラス。
    /// </summary>
    public sealed class StageNode
    {
        /// <summary>
        ///     ステージノードの初期化を行う。
        /// </summary>
        /// <param name="stageDefinition"> ステージの定義情報。 </param>
        /// <param name="stageStatus"> ステージの状態。 </param>
        public StageNode(StageDefinition stageDefinition, StageStatus stageStatus)
        {
            _definition = stageDefinition;
            _status = stageStatus;
        }

        /// <summary> ステージID。 </summary>
        public StageId Id => _definition.StageId;
        /// <summary> ステージの種類。 </summary>
        public StageType Type => _definition.StageType;
        /// <summary> ステージの状態。 </summary>
        public StageStatus Status => _status;
        /// <summary> ステージの定義情報。 </summary>

        public StageDefinition Definition => _definition;
        /// <summary> ステージに関連付けられたミッションの定義。 </summary>
        public MissionDefinition MissionDefinition => _definition.MissionDefinition;

        /// <summary>
        ///     ステージをクリア済みにする。
        /// </summary>
        public void MarkAsCleared()
        {
            if (_status == StageStatus.Unlocked)
            {
                _status = StageStatus.Cleared;
            }
        }

        /// <summary>
        ///     ステージを解放します。
        /// </summary>
        public void Unlock()
        {
            if (_status == StageStatus.Locked)
            {
                _status = StageStatus.Unlocked;
            }
        }

        private readonly StageDefinition _definition;
        private StageStatus _status;
    }
}
