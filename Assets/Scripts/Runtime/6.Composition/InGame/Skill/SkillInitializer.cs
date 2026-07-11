using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
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
using KillChord.Runtime.InfraStructure.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
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

        [SerializeField, Tooltip("スキル定義リポジトリです。")]
        private SkillRepository _skillRepository;
        [SerializeField, Tooltip("スキル演出View一覧です。")]
        private SkillView[] _skillVisuals;
        [SerializeField, Tooltip("入力進捗UI設定です。未設定時はPlayer側設定を流用します。")]
        private SkillInputProgressViewConfigAsset _inputProgressViewConfigAsset;
        [SerializeField, Tooltip("テスト用の装備スキル一覧です。未設定時はPlayer側設定を流用します。")]
        private SkillTemplateAsset[] _equippedSkills;

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

            SkillTemplate[] equippedSkills = ResolveEquippedSkills(playerModuleContainer.PlayerInitializer);
            SkillResultPresenter skillResultPresenter = new SkillResultPresenter(_skillResultViewModel);
            SkillCheckService skillCheckService = new SkillCheckService();
            SkillTargetResolver targetResolver = new SkillTargetResolver(
                targetSystemContainer.TargetSystemViewModel,
                targetSystemContainer.TargetEntityRegistry,
                playerModuleContainer.PlayerView.transform);
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
            }

            _boundPlayerView = null;
            _skillController = null;

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
            SkillInputProgressPresenter presenter = new SkillInputProgressPresenter(rowView);
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

            SkillTemplateAsset[] fallbackAssets = _equippedSkills;
            if ((fallbackAssets == null || fallbackAssets.Length == 0) && playerInitializer != null)
            {
                fallbackAssets = playerInitializer.EquippedSkillAssets;
            }

            if (fallbackAssets == null || fallbackAssets.Length == 0)
            {
                return Array.Empty<SkillTemplate>();
            }

            List<SkillTemplate> templates = new List<SkillTemplate>(fallbackAssets.Length);
            for (int i = 0; i < fallbackAssets.Length; i++)
            {
                if (fallbackAssets[i] == null)
                {
                    continue;
                }

                templates.Add(fallbackAssets[i].ToDomain());
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
        private SkillController _skillController;
        private SkillResultViewModel _skillResultViewModel;
        private SkillModuleContainer _moduleContainer;
        private PlayerView _boundPlayerView;
        private bool _isRegistered;
    }
}
