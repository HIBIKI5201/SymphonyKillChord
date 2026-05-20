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
        /// <param name="stageId"> ステージのID。</param>
        /// <param name="stageType"> ステージの種類。</param>
        /// <param name="stageStatus"> ステージの状態。</param>
        /// <param name="missionDefinition"> ステージに関連付けられたミッションの定義。</param>
        public StageNode(StageId stageId,
            StageType stageType,
            StageStatus stageStatus,
            MissionDefinition missionDefinition)
        {
            _id = stageId;
            _type = stageType;
            _status = stageStatus;
            _missionDefinition = missionDefinition;
        }

        /// <summary> ステージID。 </summary>
        public StageId Id => _id;
        /// <summary> ステージの種類。 </summary>
        public StageType Type => _type;
        /// <summary> ステージの状態。 </summary>
        public StageStatus Status => _status;
        /// <summary> ステージに関連付けられたミッションの定義。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;

        private readonly StageId _id;
        private readonly StageType _type;
        private readonly StageStatus _status;
        private readonly MissionDefinition _missionDefinition;
    }
}
