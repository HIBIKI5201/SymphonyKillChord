using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     1つのステージノード定義を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(StageNodeAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(StageNodeAsset))]
    public class StageNodeAsset : ScriptableObject
    {
        /// <summary> ステージIDの入力値です。 </summary>
        public int StageIdValue => _stageId.Id;

        /// <summary> チュートリアルステージとして設定されている場合はtrueです。 </summary>
        public bool IsTutorial => _isTutorial;

        /// <summary>
        ///     ステージノードを生成します。
        /// </summary>
        /// <returns> 生成されたステージノード。</returns>
        public StageNode Create()
        {
            if (_stageId.Id == 0)
            {
# if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageNodeAsset)}] _stageId が未設定です。", this);
#endif
                return null;
            }

            if (_stageType == StageType.Battle
                && string.IsNullOrWhiteSpace(_enemyWaveDefinitionAssetKey))
            {
                throw new System.InvalidOperationException(
                    $"バトルステージの敵Wave定義キーが未設定です。StageId: {_stageId.Id}");
            }

            var missionDefinition = _stageType == StageType.Battle && _missionDefinitionAsset != null
                ? _missionDefinitionAsset.Create()
                : null;

            var definition = new StageDefinition(
                new StageId(_stageId.Id),
                _stageType,
                _stageName,
                _flavorText,
                new StageReward(_rewardSkillBuildPoint, _rewardSkillUnlockPoint),
                _targetSceneName,
                _stageType == StageType.Battle ? _battleSceneName : string.Empty,
                _stageType == StageType.Scenario ? _scenarioId : string.Empty,
                missionDefinition,
                _isTutorial,
                _stageType == StageType.Battle ? _enemyWaveDefinitionAssetKey : string.Empty);

            // 初期解放フラグが立っている場合は Unlocked で生成する
            var initialStatus = _isInitiallyUnlocked ? StageStatus.Unlocked : StageStatus.Locked;

            return new StageNode(definition, initialStatus);
        }

#if UNITY_EDITOR
        [Header("プランナーメモ")]
        [SerializeField, TextArea, Tooltip("プランナー向けのメモ。ゲームには影響しません。")]
        private string _plannerMemo;
#endif

        [Header("基礎情報")]
        [SerializeField, Tooltip("ステージを一意に識別するID。他のノードと重複しないようにすること。")]
        [DataCategory("Stage")]
        private DataID _stageId;

        [SerializeField, Tooltip("ステージの種類。")]
        private StageType _stageType;

        [SerializeField, Tooltip("ゲーム開始時点でこのノードを解放済みにする場合はオンにする。ツリーの起点となるノードに設定すること。")]
        private bool _isInitiallyUnlocked;

        [SerializeField, Tooltip("初回起動時に自動出撃するチュートリアルステージの場合はオンにします。")]
        private bool _isTutorial;

        [Header("UI情報")]
        [SerializeField, Tooltip("ステージ名。")]
        private string _stageName;

        [SerializeField, TextArea, Tooltip("ステージのフレーバーテキスト。")]
        private string _flavorText;

        [Header("シーン遷移")]
        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名。")]
        private string _targetSceneName = "InGame";

        [Header("バトルパート情報（バトルパートの場合のみ）")]
        [SerializeField, SceneNameSelector, Tooltip("バトルパートで遷移するステージシーン名。シナリオパートは空欄にすること。")]
        private string _battleSceneName = "Stage_1";

        [SerializeField, Tooltip("バトルパートで使用する敵Wave定義のAddressablesキーです。")]
        private string _enemyWaveDefinitionAssetKey;

        [Header("シナリオパート情報（シナリオパートの場合のみ）")]
        [SerializeField, Tooltip("シナリオパートで再生するシナリオId")]
        private string _scenarioId;

        [Header("クリア報酬")]
        [SerializeField, Tooltip("スキル編成・強化に使用するポイント。")]
        private int _rewardSkillBuildPoint;

        [SerializeField, Tooltip("スキル解放・パラメーター強化に使用するポイント。")]
        private int _rewardSkillUnlockPoint;

        [Header("ミッション（バトルパートのみ）")]
        [SerializeField, Tooltip("バトルパートのミッション定義アセット。シナリオパートは空欄にすること。")]
        private MissionDefinitionAsset _missionDefinitionAsset;
    }
}
