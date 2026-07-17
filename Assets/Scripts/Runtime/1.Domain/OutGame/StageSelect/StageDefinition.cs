using KillChord.Runtime.Domain.InGame.Mission;

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
        /// <param name="targetSceneName"> ステージのターゲットシーン名。 </param>
        /// <param name="battleSceneName"> バトルパートのシーン名。 </param>
        /// <param name="scenarioId"> シナリオID。 </param>
        /// <param name="missionDefinition"> ミッション定義。 </param>
        /// <param name="isTutorial"> チュートリアルステージの場合はtrue。 </param>
        /// <param name="enemyWaveDefinitionAssetKey"> 敵Wave定義のAddressablesキー。 </param>
        public StageDefinition(
            StageId stageId,
            StageType stageType,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName,
            string battleSceneName,
            string scenarioId = "",
            MissionDefinition missionDefinition = null,
            bool isTutorial = false,
            string enemyWaveDefinitionAssetKey = "")
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                throw new System.ArgumentException("Target scene name must not be null or empty.", nameof(targetSceneName));
            }

            _stageId = stageId;
            _stageType = stageType;
            _stageName = stageName;
            _flavorText = flavorText;
            _reward = reward;
            _targetSceneName = targetSceneName;
            _battleSceneName = battleSceneName;
            _scenarioId = scenarioId ?? string.Empty;
            _missionDefinition = missionDefinition;
            _isTutorial = isTutorial;
            _enemyWaveDefinitionAssetKey = enemyWaveDefinitionAssetKey ?? string.Empty;
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
        /// <summary> ステージのターゲットシーン名。 </summary>
        public string TargetSceneName => _targetSceneName;
        /// <summary> バトルパートのシーン名。 </summary>
        public string BattleSceneName => _battleSceneName;
        /// <summary> ステージのシナリオID。 </summary>
        public string ScenarioId => _scenarioId;
        /// <summary> ステージのミッション定義。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;
        /// <summary> チュートリアルステージの場合はtrue。 </summary>
        public bool IsTutorial => _isTutorial;
        /// <summary> 敵Wave定義のAddressablesキー。 </summary>
        public string EnemyWaveDefinitionAssetKey => _enemyWaveDefinitionAssetKey;

        private readonly StageId _stageId;
        private readonly StageType _stageType;
        private readonly string _stageName;
        private readonly string _flavorText;
        private readonly StageReward _reward;
        private readonly string _targetSceneName;
        private readonly string _battleSceneName;
        private readonly string _scenarioId;
        private readonly MissionDefinition _missionDefinition;
        private readonly bool _isTutorial;
        private readonly string _enemyWaveDefinitionAssetKey;
    }
}
