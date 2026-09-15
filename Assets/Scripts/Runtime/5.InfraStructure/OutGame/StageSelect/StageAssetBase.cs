using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
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
                BuildFirstClearReward(),
                BuildReward(_clearRewards),
                _targetSceneName,
                waveDefinitionRepository);
            StageStatus initialStatus = _isInitiallyUnlocked
                ? StageStatus.Unlocked
                : StageStatus.Locked;

            return new StageNode(definition, initialStatus);
        }

        private const int LEGACY_REWARD_KIND_COUNT = 2;

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

        [Header("初回クリア報酬")]
        [SerializeField, Tooltip("初回クリア時にのみ付与するリソースと数量。")]
        private StageRewardEntry[] _firstClearRewards;

        [Header("成功報酬（毎回）")]
        [SerializeField, Tooltip("クリアするたびに付与するリソースと数量。初回クリア時は初回クリア報酬と合わせて付与する。")]
        private StageRewardEntry[] _clearRewards;

        // 旧形式の初回クリア報酬。既存アセットのシリアライズ値を保持するため残す。
        // 初回クリア報酬が未設定のアセットに限り、既知のリソースIDへ読み替えて使用する。
        // 初回クリア報酬を「なし」にしたい場合に0へ戻せるよう、Inspector には表示したままにする。
        [Header("旧形式の初回クリア報酬（移行用）")]
        [SerializeField, Min(0), Tooltip("旧形式の改造ポイント。上の「初回クリア報酬」が空のときだけ使われる。初回クリア報酬をなしにする場合は0にすること。")]
        private int _rewardSkillBuildPoint;

        [SerializeField, Min(0), Tooltip("旧形式の研究ポイント。上の「初回クリア報酬」が空のときだけ使われる。初回クリア報酬をなしにする場合は0にすること。")]
        private int _rewardSkillUnlockPoint;

        /// <summary>
        ///     ステージ固有の定義情報を生成する。
        /// </summary>
        /// <param name="stageId"> ステージID。</param>
        /// <param name="stageName"> ステージ名。</param>
        /// <param name="flavorText"> フレーバーテキスト。</param>
        /// <param name="firstClearReward"> 初回クリア報酬。</param>
        /// <param name="clearReward"> 毎回の成功報酬。</param>
        /// <param name="targetSceneName"> 遷移先シーン名。</param>
        /// <param name="waveDefinitionRepository"> バトルシーン名の解決に使う敵Wave定義リポジトリ。</param>
        /// <returns> 生成したステージ定義。</returns>
        protected abstract StageDefinition CreateDefinition(
            StageId stageId,
            string stageName,
            string flavorText,
            StageReward firstClearReward,
            StageReward clearReward,
            string targetSceneName,
            IEnemyWaveDefinitionRepository waveDefinitionRepository);

        /// <summary>
        ///     初回クリア報酬を生成する。
        ///     新形式が未設定の場合は、旧形式のポイント設定を既知のリソースIDへ読み替える。
        /// </summary>
        /// <returns> 初回クリア報酬。</returns>
        private StageReward BuildFirstClearReward()
        {
            if (_firstClearRewards != null && _firstClearRewards.Length > 0)
            {
                return BuildReward(_firstClearRewards);
            }

            List<GameResourceAmount> legacyItems = new(LEGACY_REWARD_KIND_COUNT);
            if (_rewardSkillBuildPoint > 0)
            {
                legacyItems.Add(new GameResourceAmount(GameResourceIds.SkillLevelupPoint, _rewardSkillBuildPoint));
            }

            if (_rewardSkillUnlockPoint > 0)
            {
                legacyItems.Add(new GameResourceAmount(GameResourceIds.ResearchPoint, _rewardSkillUnlockPoint));
            }

            return new StageReward(legacyItems);
        }

        /// <summary>
        ///     入力された報酬エントリから報酬を生成する。ID未設定や数量0のエントリは無視する。
        /// </summary>
        /// <param name="entries"> 報酬エントリの一覧。</param>
        /// <returns> 生成した報酬。</returns>
        private static StageReward BuildReward(StageRewardEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return StageReward.Empty;
            }

            List<GameResourceAmount> items = new(entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                StageRewardEntry entry = entries[i];
                if (entry.ResourceIdValue == 0 || entry.Amount <= 0)
                {
                    continue;
                }

                items.Add(new GameResourceAmount(new GameResourceId(entry.ResourceIdValue), entry.Amount));
            }

            return new StageReward(items);
        }
    }
}
