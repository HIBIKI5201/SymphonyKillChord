using KillChord.Runtime.Domain.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの定義情報を表すクラス。
    ///     ステージ詳細画面に表示する情報を保持する。
    /// </summary>
    public sealed class StageDefinition
    {
        /// <summary>
        ///     ステージの定義情報を初期化する。
        /// </summary>
        /// <param name="stageId"> ステージの ID。 </param>
        /// <param name="stageType"> ステージの種類。 </param>
        /// <param name="stageName"> ステージの名前。 </param>
        /// <param name="flavorText"> ステージのフレーバーテキスト。 </param>
        /// <param name="reward"> ステージの報酬情報。 </param>
        /// <param name="missionDefinition"> ステージのミッション定義。 </param>
        public StageDefinition(StageId stageId, StageType stageType, string stageName, string flavorText,
            StageReward reward, MissionDefinition missionDefinition = null)
        {
            _stageId = stageId;
            _stageType = stageType;
            _stageName = stageName;
            _flavorText = flavorText;
            _reward = reward;
            _missionDefinition = missionDefinition;
        }

        /// <summary> ステージの ID。 </summary>
        public StageId StageId => _stageId;
        /// <summary> ステージの種類。 </summary>
        public StageType StageType => _stageType;
        /// <summary> ステージの名前。 </summary>
        public string StageName => _stageName;
        /// <summary> ステージのフレーバーテキスト。 </summary>
        public string FlavorText => _flavorText;
        /// <summary> ステージの報酬情報。 </summary>
        public StageReward Reward => _reward;
        /// <summary> ステージのミッション定義。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;

        private readonly StageId _stageId;
        private readonly StageType _stageType;
        private readonly string _stageName;
        private readonly string _flavorText;
        private readonly StageReward _reward;
        private readonly MissionDefinition _missionDefinition;
    }
}
