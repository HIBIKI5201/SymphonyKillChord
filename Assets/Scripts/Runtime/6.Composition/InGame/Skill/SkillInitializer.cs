using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Application.Player.SkillEffect;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Composition.InGame.UI;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Skill
{
    /// <summary>
    ///     Skillモジュールを初期化して公開するモジュールです。
    /// </summary>
    public sealed class SkillInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SkillInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 450;

        [SerializeField, Tooltip("スキル演出View一覧です。")]
        private SkillView[] _skillVisuals;
        [SerializeField, Tooltip("入力進捗UI設定です。未設定時はPlayer側設定を流用します。")]
        private SkillInputProgressUIConfig _inputProgressUIConfig;
        [SerializeField, SourceDataCollection("Skill")]
        [Tooltip("テスト用の装備スキルID一覧です。未設定時はPlayer側設定を流用します。")]
        private DataID[] _equippedSkills;
        [SerializeField, SourceDataAddress]
        [Tooltip("改造画面を経由していない場合に、セーブデータから装備スキルを解決するためのリポジトリの Addressables キーです。")]
        private string _skillBuildRepositoryKey;
        [SerializeField, SourceDataAddress]
        [Tooltip("テスト用装備スキルIDの解決に使うスキルリポジトリの Addressables キーです。")]
        private string _skillRepositoryKey;

        /// <summary>
        ///     改造画面を経由せずシーンへ入った場合に備え、セーブデータ由来の装備スキルを非同期で解決します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_skillRepositoryKey))
            {
                _loadedSkillRepository = await _skillRepositoryKey.LoadAssetAsync<SkillRepository>(this, cancellationToken);
            }

            if (ServiceLocator.TryGetInstance(out SkillBuildDefinition _)
                || string.IsNullOrWhiteSpace(_skillBuildRepositoryKey))
            {
                // 改造画面経由で既にSkillBuildDefinitionが登録済み、
                // またはキー未設定の場合はセーブデータの再ロードを行わない。
                return true;
            }

            try
            {
                SkillBuildRepository skillBuildRepository =
                    await _skillBuildRepositoryKey.LoadAssetAsync<SkillBuildRepository>(this, cancellationToken);
                IReadOnlyList<EquippedSkill> equippedSkills = await skillBuildRepository.GetEquippedSkills();
                _saveDataEquippedSkills = ToSkillTemplates(equippedSkills);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(SkillInitializer)}] セーブデータ由来の装備スキル解決に失敗しました: {exception}", this);
            }

            return true;
        }

        /// <summary>
        ///     スキルモジュールのContainerを登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _skillResultViewModel = new SkillResultViewModel();
            SkillResultView skillResultView = FindAnyObjectByType<SkillResultView>();
            skillResultView?.Bind(_skillResultViewModel);

            _moduleContainer = new SkillModuleContainer(_skillResultViewModel);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     他モジュールと結合してスキルを初期化します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            if (playerModuleContainer == null)
            {
                Debug.LogError($"[{nameof(SkillInitializer)}] {nameof(PlayerModuleContainer)} が見つかりません。", this);
                return false;
            }

            TargetSystemModuleContainer targetSystemContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            if (targetSystemContainer == null)
            {
                Debug.LogError($"[{nameof(SkillInitializer)}] {nameof(TargetSystemModuleContainer)} が見つかりません。", this);
                return false;
            }

            MusicSyncModuleContainer musicSyncContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            if (musicSyncContainer == null || musicSyncContainer.MusicSyncState == null || musicSyncContainer.MusicSyncService == null)
            {
                Debug.LogError($"[{nameof(SkillInitializer)}] MusicSyncモジュールが見つかりません。", this);
                return false;
            }

            _skillInputProgressUIInitializer = ServiceLocator.GetInstance<SkillInputProgressUIInitializer>();
            if (_skillInputProgressUIInitializer == null)
            {
                Debug.LogError($"[{nameof(SkillInitializer)}] {nameof(SkillInputProgressUIInitializer)} が見つかりません。", this);
                return false;
            }

            _skillCrosshairProgressUIInitializer = ServiceLocator.GetInstance<SkillCrosshairProgressUIInitializer>();
            _skillListUIInitializer = ServiceLocator.GetInstance<SkillListUIInitializer>();

            SkillTemplate[] equippedSkills = ResolveEquippedSkills(playerModuleContainer.PlayerInitializer);
            SkillResultPresenter skillResultPresenter = new SkillResultPresenter(_skillResultViewModel);
            SkillCheckService skillCheckService = new SkillCheckService();
            SkillTargetResolver targetResolver = new SkillTargetResolver(
                targetSystemContainer.TargetSystemViewModel,
                targetSystemContainer.TargetEntityRegistry,
                targetSystemContainer.TargetAreaQuery,
                playerModuleContainer.PlayerView.transform,
                playerModuleContainer.PlayerStatusBonus.AreaAttackRangeAddition);
            SkillAttackController skillAttackController = new SkillAttackController(playerModuleContainer.PlayerEntity, targetResolver);
            SkillEffectExecutorResolver effectExecutorResolver = new SkillEffectExecutorResolver();
            SkillEffectExecutorFactory.RegisterDefaults(effectExecutorResolver, skillAttackController);
            SkillUsecase skillUsecase = new SkillUsecase(targetResolver, effectExecutorResolver, playerModuleContainer.PlayerEntity);

            _skillController = new SkillController(musicSyncContainer.MusicSyncService);
            _skillController.Initialize(BuildSkillExecutionControllers(
                equippedSkills,
                ResolveSkillVisuals(playerModuleContainer.PlayerInitializer),
                musicSyncContainer.MusicSyncState,
                skillResultPresenter,
                skillCheckService,
                skillUsecase));
            _skillController.OnSkillAnimationRequested += playerModuleContainer.PlayerView.PlaySkillAnimation;
            _skillController.OnSkillVoiceRequested += playerModuleContainer.PlayerView.PlaySkillVoice;
            _boundPlayerView = playerModuleContainer.PlayerView;
            _moduleContainer.SetSkillController(_skillController);
            return true;
        }

        /// <summary>
        ///     登録済みサービスとイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_skillController != null && _boundPlayerView != null)
            {
                _skillController.OnSkillAnimationRequested -= _boundPlayerView.PlaySkillAnimation;
                _skillController.OnSkillVoiceRequested -= _boundPlayerView.PlaySkillVoice;
            }

            _boundPlayerView = null;
            _skillController = null;
            _saveDataEquippedSkills = null;
            _skillBuildRepositoryKey.ReleaseLoadedAsset(this);
            _skillRepositoryKey.ReleaseLoadedAsset(this);
            _loadedSkillRepository = null;

            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<SkillModuleContainer>();
            _moduleContainer = null;
            _isRegistered = false;
        }

        /// <summary>
        ///     装備中スキルの実行Controller一覧を構築します。
        /// </summary>
        private SkillExecutionController[] BuildSkillExecutionControllers(
            IReadOnlyList<SkillTemplate> equippedSkills,
            IReadOnlyList<SkillView> skillVisuals,
            MusicSyncState musicSyncState,
            SkillResultPresenter skillResultPresenter,
            SkillCheckService checkService,
            SkillUsecase skillUsecase)
        {
            if (equippedSkills == null || equippedSkills.Count == 0)
            {
                return Array.Empty<SkillExecutionController>();
            }

            List<SkillExecutionController> executionControllers = new List<SkillExecutionController>(equippedSkills.Count);
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                SkillTemplate skillTemplate = equippedSkills[i];
                if (skillTemplate == null)
                {
                    continue;
                }

                SkillDefinition definition = skillTemplate.ToSkillDefinition(musicSyncState.Bpm);
                SkillView view = FindSkillView(skillVisuals, definition.Id.Value);
                if (view == null)
                {
                    continue;
                }

                SkillCooldownState cooldownState = new SkillCooldownState(definition);
                SkillRhythmState rhythmState = new SkillRhythmState(definition.SkillPattern.Signatures.Length * 2);
                SkillInputProgressController progressController = BuildSkillProgressModules(definition);
                SkillExecutionController executionController = new SkillExecutionController(
                    skillResultPresenter,
                    progressController,
                    cooldownState,
                    skillUsecase,
                    checkService,
                    view,
                    definition,
                    rhythmState);
                executionControllers.Add(executionController);
            }

            return executionControllers.ToArray();
        }

        /// <summary>
        ///     装備中1スキル分の入力進捗UIモジュール一式を構築する。
        /// </summary>
        private SkillInputProgressController BuildSkillProgressModules(SkillDefinition definition)
        {
            ISkillInputProgressRowView rowView = _skillInputProgressUIInitializer.CreateInputProgressRow(definition);

            ISkillCrosshairProgressView crosshairView = null;
            SkillCrosshairProgressController crosshairController = null;
            if (_skillCrosshairProgressUIInitializer != null)
            {
                crosshairView = _skillCrosshairProgressUIInitializer.CreateCrosshairProgressView(definition);
                crosshairController = _skillCrosshairProgressUIInitializer.Controller;
            }

            ISkillInputProgressRowView listRowView = _skillListUIInitializer != null
                ? _skillListUIInitializer.CreateSkillListRow(definition)
                : null;

            SkillInputProgressPresenter presenter = new SkillInputProgressPresenter(
                rowView,
                crosshairView,
                crosshairController,
                _skillInputProgressUIInitializer.GuideProgressController,
                listRowView);
            SkillInputProgressState state = new SkillInputProgressState(definition);
            return new SkillInputProgressController(state, presenter);
        }

        /// <summary>
        ///     装備中スキル一覧を解決します。
        /// </summary>
        private SkillTemplate[] ResolveEquippedSkills(PlayerInitializer playerInitializer)
        {
            if (ServiceLocator.TryGetInstance(out SkillBuildDefinition buildDefinition) &&
                buildDefinition.EquippedSkills != null &&
                buildDefinition.EquippedSkills.Count > 0)
            {
                List<SkillTemplate> buildSkills = new List<SkillTemplate>(buildDefinition.EquippedSkills.Count);
                for (int i = 0; i < buildDefinition.EquippedSkills.Count; i++)
                {
                    EquippedSkill equippedSkill = buildDefinition.EquippedSkills[i];
                    if (!equippedSkill.HasSkill)
                    {
                        continue;
                    }

                    buildSkills.Add(equippedSkill.SkillTemplate);
                }

                if (buildSkills.Count > 0)
                {
                    return buildSkills.ToArray();
                }
            }

            if (_saveDataEquippedSkills != null && _saveDataEquippedSkills.Length > 0)
            {
                return _saveDataEquippedSkills;
            }

            SkillId[] fallbackIds = ConvertToSkillIds(_equippedSkills);
            if ((fallbackIds == null || fallbackIds.Length == 0) && playerInitializer != null)
            {
                fallbackIds = playerInitializer.EquippedSkillIds;
            }

            if (fallbackIds == null || fallbackIds.Length == 0 || _loadedSkillRepository == null)
            {
                return Array.Empty<SkillTemplate>();
            }

            List<SkillTemplate> templates = new List<SkillTemplate>(fallbackIds.Length);
            for (int i = 0; i < fallbackIds.Length; i++)
            {
                if (_loadedSkillRepository.TryGetSkill(fallbackIds[i], out SkillTemplate template))
                {
                    templates.Add(template);
                }
            }

            return templates.ToArray();
        }

        /// <summary>
        ///     DataID配列をSkillId配列へ変換します。
        /// </summary>
        /// <param name="dataIds"> 変換元のDataID配列です。 </param>
        /// <returns> 変換後のSkillId配列です。 </returns>
        private static SkillId[] ConvertToSkillIds(DataID[] dataIds)
        {
            if (dataIds == null || dataIds.Length == 0)
            {
                return Array.Empty<SkillId>();
            }

            List<SkillId> ids = new List<SkillId>(dataIds.Length);
            for (int i = 0; i < dataIds.Length; i++)
            {
                if (dataIds[i].Id == 0)
                {
                    continue;
                }

                ids.Add(new SkillId(dataIds[i].Id));
            }

            return ids.ToArray();
        }

        /// <summary>
        ///     セーブデータから取得した装備スキル一覧を SkillTemplate の配列へ変換します。
        /// </summary>
        /// <param name="equippedSkills"> 変換元の装備スキル一覧です。 </param>
        /// <returns> 変換後の SkillTemplate 配列です。 </returns>
        private static SkillTemplate[] ToSkillTemplates(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            if (equippedSkills == null || equippedSkills.Count == 0)
            {
                return Array.Empty<SkillTemplate>();
            }

            List<SkillTemplate> templates = new List<SkillTemplate>(equippedSkills.Count);
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                if (!equippedSkills[i].HasSkill)
                {
                    continue;
                }

                templates.Add(equippedSkills[i].SkillTemplate);
            }

            return templates.ToArray();
        }

        /// <summary>
        ///     使用するスキル演出View一覧を解決します。
        /// </summary>
        private SkillView[] ResolveSkillVisuals(PlayerInitializer playerInitializer)
        {
            if (_skillVisuals != null && _skillVisuals.Length > 0)
            {
                return _skillVisuals;
            }

            return playerInitializer != null ? playerInitializer.SkillVisuals : Array.Empty<SkillView>();
        }

        /// <summary>
        ///     スキルIDに対応する演出Viewを検索します。
        /// </summary>
        private SkillView FindSkillView(IReadOnlyList<SkillView> skillVisuals, int skillId)
        {
            if (skillVisuals == null)
            {
                return null;
            }

            for (int i = 0; i < skillVisuals.Count; i++)
            {
                if (skillVisuals[i] != null && skillVisuals[i].Id == skillId)
                {
                    return skillVisuals[i];
                }
            }

            return null;
        }

        private SkillInputProgressUIInitializer _skillInputProgressUIInitializer;
        private SkillCrosshairProgressUIInitializer _skillCrosshairProgressUIInitializer;
        private SkillListUIInitializer _skillListUIInitializer;
        private SkillController _skillController;
        private SkillResultViewModel _skillResultViewModel;
        private SkillModuleContainer _moduleContainer;
        private PlayerView _boundPlayerView;
        private bool _isRegistered;
        private SkillTemplate[] _saveDataEquippedSkills;
        private SkillRepository _loadedSkillRepository;
    }
}
