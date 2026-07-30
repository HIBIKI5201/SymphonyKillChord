using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ共通情報を保持するScriptableObjectの抽象基底クラス。
    /// </summary>
    public abstract class StageAssetBase : ScriptableObject
    {
        /// <summary> ステージIDの入力値。 </summary>
        public int StageIdValue => _stageId.Id;
        /// <summary> チュートリアルステージとして設定されている場合はtrue。 </summary>
        public virtual bool IsTutorial => false;

        /// <summary>
        ///     ステージノードを生成する。
        /// </summary>
        /// <param name="waveDefinitionRepository"> バトルシーン名の解決に使う敵Wave定義リポジトリ。</param>
        /// <returns> 生成したステージノード。</returns>
        public StageNode Create(IEnemyWaveDefinitionRepository waveDefinitionRepository)
        {
            if (_stageId.Id == 0)
            {
                throw new System.InvalidOperationException(
                    $"[{nameof(StageAssetBase)}] StageIdが未設定です。Asset: {name}");
            }

            StageDefinition definition = CreateDefinition(
                new StageId(_stageId.Id),
                _stageName,
                _flavorText,
                new StageReward(_rewardSkillBuildPoint, _rewardSkillUnlockPoint),
                _targetSceneName,
                waveDefinitionRepository);
            StageStatus initialStatus = _isInitiallyUnlocked
                ? StageStatus.Unlocked
                : StageStatus.Locked;

            return new StageNode(definition, initialStatus);
        }

#if UNITY_EDITOR
        [Header("プランナーメモ")]
        [SerializeField, TextArea, Tooltip("プランナー向けのメモ。ゲームには影響しません。")]
        private string _plannerMemo;
#endif

        [Header("基礎情報")]
        [SerializeField, Tooltip("ステージを一意に識別するID。他のノードと重複しないようにすること。")]
        [SourceDataCollection("StageAsset")]
        private DataID _stageId;

        [SerializeField, Tooltip("ゲーム開始時点でこのノードを解放済みにする場合はオンにする。")]
        private bool _isInitiallyUnlocked;

        [Header("UI情報")]
        [SerializeField, Tooltip("ステージ名。")]
        private string _stageName;

        [SerializeField, TextArea, Tooltip("ステージのフレーバーテキスト。")]
        private string _flavorText;

        [Header("シーン遷移")]
        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名。")]
        private string _targetSceneName = "InGame";

        [Header("クリア報酬")]
        [SerializeField, Tooltip("スキル編成・強化に使用するポイント。")]
        private int _rewardSkillBuildPoint;

        [SerializeField, Tooltip("スキル解放・パラメーター強化に使用するポイント。")]
        private int _rewardSkillUnlockPoint;

        /// <summary>
        ///     ステージ固有の定義情報を生成する。
        /// </summary>
        /// <param name="stageId"> ステージID。</param>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="reward"> クリア報酬。</param>
        /// <param name="targetSceneName"> 遷移先シーン名。</param>
        /// <param name="waveDefinitionRepository"> バトルシーン名の解決に使う敵Wave定義リポジトリ。</param>
        /// <returns> 生成したステージ定義。</returns>
        protected abstract StageDefinition CreateDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward reward,
            string targetSceneName,
            IEnemyWaveDefinitionRepository waveDefinitionRepository);
    }
}
