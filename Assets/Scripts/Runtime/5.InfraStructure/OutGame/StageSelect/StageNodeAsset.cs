using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Constant;
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
        /// <summary>
        ///     ステージノードを生成します。
        /// </summary>
        /// <returns> 生成されたステージノード。</returns>
        public StageNode Create()
        {
            if (string.IsNullOrEmpty(_stageId))
            {
# if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageNodeAsset)}] _stageId が設定されていません。", this);
#endif
                return null;
            }

            var missionDefinition = _stageType == StageType.Battle && _missionDefinitionAsset != null
                ? _missionDefinitionAsset.Create()
                : null;

            var definition = new StageDefinition(
                new StageId(_stageId),
                _stageType,
                _stageName,
                _flavorText,
                new StageReward(_rewardSkillBuildPoint, _rewardSkillUnlockPoint),
                missionDefinition);

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
        private string _stageId;

        [SerializeField, Tooltip("ステージの種類。")]
        private StageType _stageType;

        [SerializeField, Tooltip("ゲーム開始時点でこのノードを解放済みにする場合はオンにする。ツリーの起点となるノードに設定すること。")]
        private bool _isInitiallyUnlocked;

        [Header("UI情報")]
        [SerializeField, Tooltip("ステージ名。")]
        private string _stageName;

        [SerializeField, TextArea, Tooltip("ステージのフレーバーテキスト。")]
        private string _flavorText;

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