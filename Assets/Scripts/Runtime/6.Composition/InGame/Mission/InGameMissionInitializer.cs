using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Skill;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.InfraStructure.OutGame.Scenario;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Combo;
using KillChord.Runtime.View.InGame.Mission;
using KillChord.Runtime.View.OutGame.Scenario;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using AnimationEventData = KillChord.Runtime.Domain.OutGame.Scenario.AnimationEvent;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     インゲームにおけるミッションシステムの初期化を行うクラス。
    /// </summary>
    public class InGameMissionInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(InGameMissionInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 600;

        /// <summary>
        ///     OutGameで選択されたミッションIDから、ミッション定義を非同期で解決します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (!ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState)
                || !selectedMissionState.HasSelectedMission)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] OutGameでミッションが選択されていません。",
                    this);
                return false;
            }

            _loadedMissionDefinitionRepository =
                await _missionDefinitionRepositoryKey.LoadAssetAsync<MissionDefinitionRepository>(this, cancellationToken);
            _loadedEnemyMissionKeyRepository =
                await _enemyMissionKeyRepositoryKey.LoadAssetAsync<EnemyMissionKeyRepository>(this, cancellationToken);

            if (_loadedMissionDefinitionRepository == null || _loadedEnemyMissionKeyRepository == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] ミッション関連リポジトリのロードに失敗しました。",
                    this);
                return false;
            }

            if (!_loadedMissionDefinitionRepository.TryCreateMissionDefinition(
                    selectedMissionState.CurrentMissionId,
                    _loadedEnemyMissionKeyRepository,
                    out _resolvedMissionDefinition))
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] ミッションIDに対応する定義が見つかりません。",
                    this);
                return false;
            }

            if (!RequiresScenarioPlayback())
            {
                return true;
            }

            try
            {
                _loadedBackgroundCatalog =
                    await _backgroundCatalogKey.LoadAssetAsync<BackgroundCatalogAsset>(this, cancellationToken);
                _loadedAnimationCatalog =
                    await _animationCatalogKey.LoadAssetAsync<AnimationCatalogAsset>(this, cancellationToken);
                _loadedPortraitCatalog =
                    await _portraitCatalogKey.LoadAssetAsync<PortraitCatalogAsset>(this, cancellationToken);
                _loadedScenarioSettings =
                    await _scenarioSettingsKey.LoadAssetAsync<ScenarioSettingsAsset>(this, cancellationToken);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return false;
            }

            if (_loadedBackgroundCatalog == null
                || _loadedAnimationCatalog == null
                || _loadedPortraitCatalog == null
                || _loadedScenarioSettings == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] シナリオ再生に必要なアセットのロードに失敗しました。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     ミッションシステムを構築してContainerを登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!TryBuildScenarioPlayback())
            {
                return false;
            }

            if (!TryInitialize(out MissionRuntimeService missionRuntimeService))
            {
                return false;
            }

            MissionEventController missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (missionEventController == null)
            {
                Debug.LogError($"[{nameof(InGameMissionInitializer)}] {nameof(MissionEventController)} を取得できませんでした。", this);
                return false;
            }

            _moduleContainer = new MissionModuleContainer(missionRuntimeService, missionEventController);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;
            return true;
        }

        /// <summary>
        ///     プレイヤー戦闘イベントとミッション実績記録を結合します。
        /// </summary>
        /// <returns> 結合に成功した場合はtrueです。 </returns>
        public override bool Ready()
        {
            PlayerModuleContainer playerModuleContainer =
                ServiceLocator.GetInstance<PlayerModuleContainer>();
            SkillModuleContainer skillModuleContainer =
                ServiceLocator.GetInstance<SkillModuleContainer>();
            if (playerModuleContainer == null
                || playerModuleContainer.PlayerEntity == null
                || playerModuleContainer.PlayerController == null
                || playerModuleContainer.PlayerAttackController == null
                || skillModuleContainer?.SkillController == null
                || !ServiceLocator.TryGetInstance(out TargetSystemController targetSystemController))
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] プレイヤー戦闘モジュールを取得できませんでした。",
                    this);
                return false;
            }

            MissionProgressRecorderController recorderController =
                new MissionProgressRecorderController(
                    _moduleContainer.MissionRuntimeService.MissionProgress,
                    _moduleContainer.MissionEventController,
                    _comboHudPresenter);
            recorderController.Bind(
                playerModuleContainer.PlayerEntity,
                playerModuleContainer.PlayerController,
                playerModuleContainer.PlayerAttackController,
                skillModuleContainer.SkillController,
                targetSystemController);
            _recorderController = recorderController;

            if (_missionStepPopupView != null)
            {
                _popupController = new MissionStepPopupController(
                    _moduleContainer.MissionRuntimeService,
                    _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                    _missionStepPopupView,
                    playerModuleContainer.InputSuppressionState,
                    _popupInputSuppressionDuration);
            }
            _playerBuffController = new MissionPlayerBuffController(
                _moduleContainer.MissionRuntimeService,
                _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                playerModuleContainer.PlayerEntity);

            _stepEntryActionController = new MissionStepEntryActionController(
                _moduleContainer.MissionRuntimeService,
                _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                new IMissionStepEntryActionExecutor[]
                {
                    new SetSkillExecutionEnabledStepEntryActionExecutor(playerModuleContainer.PlayerActionRestrictionState),
                    new ToggleEnemyBattleAiStepEntryActionExecutor(ServiceLocator.GetInstance<EnemyModuleContainer>().EnemyBattleAIRegistry)
                });

            if (_scenarioUsecase != null
                && !TryInitializeMissionScenarioController())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="missionRuntimeService"> 構築したミッション実行サービスです。 </param>
        /// <returns> 初期化に成功した場合はtrue。 </returns>
        public bool TryInitialize(out MissionRuntimeService missionRuntimeService)
        {
            missionRuntimeService = null;

            if (!ValidateReferences())
            {
                return false;
            }

            if (_resolvedMissionDefinition == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    "ミッション定義が解決されていません。",
                    this);

                return false;
            }

            MissionDefinition definition = _resolvedMissionDefinition;
            MissionProgress progress = new MissionFactory().CreateMissionProgress();

            missionRuntimeService = new MissionRuntimeService(
                definition,
                progress,
                new MissionTimeAdvanceUsecase(),
                new MissionEnemyKilledUsecase(),
                new MissionActionPerformedUsecase(),
                new MissionPlayerDeadUsecase(),
                new MissionRuleRunner(definition),
                new MissionEvaluationRunner());

            MissionHudViewModel missionHudViewModel = new MissionHudViewModel();

            MissionHudPresenter missionHudPresenter = new MissionHudPresenter(
                missionRuntimeService,
                missionHudViewModel);

            MissionEventController missionEventController = new MissionEventController(
                missionRuntimeService,
                missionHudPresenter);

            ComboHudViewModel comboHudViewModel = new ComboHudViewModel();
            _comboHudPresenter = new ComboHudPresenter(comboHudViewModel);

            _missionHudView.Initialize(missionHudViewModel);
            _missionLoopView.Initialize(missionEventController);
            _comboHudView.Initialize(comboHudViewModel, _comboVisibleCount);

            missionHudPresenter.Present();

            ServiceLocator.RegisterInstance(missionRuntimeService);
            ServiceLocator.RegisterInstance(missionEventController);

            _registeredMissionRuntimeService = true;
            _registeredMissionEventController = true;

            return true;
        }

        /// <summary>
        ///     登録済みContainerを解除します。
        /// </summary>
        public override void Shutdown()
        {
            _recorderController?.Dispose();
            _popupController?.Dispose();
            _playerBuffController?.Dispose();
            _stepEntryActionController?.Dispose();
            if (_scenarioController != null)
            {
                _scenarioController.OnScenarioPlaybackStarted -= HandleScenarioPlaybackStarted;
                _scenarioController.OnScenarioPlaybackEnded -= HandleScenarioPlaybackEnded;
                _scenarioController.Dispose();
                _scenarioController = null;
            }

            SetScenarioDisplayActive(false);

            _missionDefinitionRepositoryKey.ReleaseLoadedAsset(this);
            _enemyMissionKeyRepositoryKey.ReleaseLoadedAsset(this);
            _backgroundCatalogKey.ReleaseLoadedAsset(this);
            _animationCatalogKey.ReleaseLoadedAsset(this);
            _portraitCatalogKey.ReleaseLoadedAsset(this);
            _scenarioSettingsKey.ReleaseLoadedAsset(this);
            _loadedMissionDefinitionRepository = null;
            _loadedEnemyMissionKeyRepository = null;
            _resolvedMissionDefinition = null;
            _loadedBackgroundCatalog = null;
            _loadedAnimationCatalog = null;
            _loadedPortraitCatalog = null;
            _loadedScenarioSettings = null;
            _scenarioUsecase = null;
            _scenarioInputController = null;
            _scenarioViewModel = null;

            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<MissionModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
        }

        [SerializeField, Tooltip("ミッション情報を表示するHUDのビュー。")] private MissionHudView _missionHudView;
        [SerializeField, Tooltip("ミッションの更新処理を行うループのビュー。")] private MissionLoopView _missionLoopView;
        [SerializeField, Tooltip("目標ステップの説明ポップアップを表示するビュー。未設定の場合はポップアップ機能を使用しない。")] private MissionStepPopupView _missionStepPopupView;
        [SerializeField, Tooltip("現在のコンボ数を表示するビュー。")] private ComboHudView _comboHudView;
        [SerializeField, Min(0f), Tooltip("説明ポップアップ表示直後にプレイヤー入力を無効化する秒数。")] private float _popupInputSuppressionDuration = MissionStepPopupController.DefaultInputSuppressionDuration;
        [SerializeField, SourceDataAddress, Tooltip("ミッション定義リポジトリの Addressables キーです。")]
        private string _missionDefinitionRepositoryKey;
        [SerializeField, SourceDataAddress, Tooltip("敵ミッションキーリポジトリの Addressables キーです。")]
        private string _enemyMissionKeyRepositoryKey;
        [SerializeField, Tooltip("シナリオ表示と入力をまとめて有効化するルート。ScenarioViewとScenarioInputViewを子に配置します。")]
        private GameObject _scenarioRoot;
        [SerializeField, Tooltip("インゲームで使用するシナリオ表示View。ScenarioPlaybackClearConditionを使う場合に必須です。")]
        private ScenarioView _scenarioView;
        [SerializeField, Tooltip("インゲームで使用するシナリオ入力View。ScenarioPlaybackClearConditionを使う場合に必須です。")]
        private ScenarioInputView _scenarioInputView;
        [SerializeField, SourceDataAddress, Tooltip("背景カタログの Addressables キーです。")]
        private string _backgroundCatalogKey = "BackgroundCatalogAsset";
        [SerializeField, SourceDataAddress, Tooltip("アニメーションカタログの Addressables キーです。")]
        private string _animationCatalogKey = "AnimationCatalogAsset";
        [SerializeField, SourceDataAddress, Tooltip("立ち絵カタログの Addressables キーです。")]
        private string _portraitCatalogKey = "PortraitCatalogAsset";
        [SerializeField, SourceDataAddress, Tooltip("シナリオ設定の Addressables キーです。")]
        private string _scenarioSettingsKey = "ScenarioSettingsAsset";
        [SerializeField, Min(0), Tooltip("コンボ数が表示される最小値。")]
        private int _comboVisibleCount = 1;

        private bool _registeredMissionRuntimeService;
        private bool _registeredMissionEventController;
        private bool _isModuleRegistered;
        private MissionModuleContainer _moduleContainer;
        private MissionProgressRecorderController _recorderController;
        private MissionStepPopupController _popupController;
        private MissionPlayerBuffController _playerBuffController;
        private MissionStepEntryActionController _stepEntryActionController;
        private MissionScenarioController _scenarioController;
        private ComboHudPresenter _comboHudPresenter;
        private MissionDefinitionRepository _loadedMissionDefinitionRepository;
        private EnemyMissionKeyRepository _loadedEnemyMissionKeyRepository;
        private MissionDefinition _resolvedMissionDefinition;
        private BackgroundCatalogAsset _loadedBackgroundCatalog;
        private AnimationCatalogAsset _loadedAnimationCatalog;
        private PortraitCatalogAsset _loadedPortraitCatalog;
        private ScenarioSettingsAsset _loadedScenarioSettings;
        private ScenarioUsecase _scenarioUsecase;
        private ScenarioInputController _scenarioInputController;
        private ViewModel _scenarioViewModel;

        /// <summary>
        ///     Mission定義にシナリオ再生ステップがあるか確認します。
        /// </summary>
        /// <returns>シナリオ再生ステップがある場合はtrue</returns>
        private bool RequiresScenarioPlayback()
        {
            return _resolvedMissionDefinition != null
                && _resolvedMissionDefinition.ClearCondition.HasStepWithCondition<ScenarioPlaybackClearCondition>();
        }

        /// <summary>
        ///     Scenarioモジュールの既存実装を使い、インゲーム用の再生スタックを構築します。
        /// </summary>
        /// <returns>構築に成功した場合はtrue</returns>
        private bool TryBuildScenarioPlayback()
        {
            if (!RequiresScenarioPlayback())
            {
                return true;
            }

            if (!ValidateScenarioPlaybackReferences())
            {
                return false;
            }

            ScenarioAdvanceGate advanceGate = new();
            _scenarioViewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new();
            IScenarioRepository scenarioRepository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_loadedBackgroundCatalog);
            IAnimationRepository animationRepository = new AnimationRepository(_loadedAnimationCatalog);
            IPortraitRepository portraitRepository = new PortraitRepository(_loadedPortraitCatalog);
            IScenarioSettingsRepository scenarioSettingsRepository = new ScenarioSettingsRepository(_loadedScenarioSettings);

            TextPresenter textPresenter = new(_scenarioViewModel);
            FadePresenter fadePresenter = new(_scenarioViewModel);
            BackgroundPresenter backgroundPresenter = new(_scenarioViewModel);
            AnimationPresenter animationPresenter = new(_scenarioViewModel);
            PortraitPresenter portraitPresenter = new(_scenarioViewModel);
            LayerPresenter layerPresenter = new(_scenarioViewModel);
            ScenarioPresenterFacade presenterFacade = new(
                textPresenter,
                fadePresenter,
                backgroundPresenter,
                animationPresenter,
                portraitPresenter,
                layerPresenter,
                _scenarioViewModel);

            _scenarioUsecase = new ScenarioUsecase(
                scenarioRepository,
                handlerRepo,
                advanceGate,
                presenterFacade,
                scenarioSettingsRepository);
            _scenarioInputController = new ScenarioInputController(
                advanceGate,
                _scenarioUsecase,
                _scenarioUsecase);

            TextEventHandler textEventHandler = new(
                presenterFacade,
                _scenarioUsecase,
                _scenarioUsecase,
                scenarioSettingsRepository);
            FadeEventHandler fadeEventHandler = new(presenterFacade);
            BackgroundEventHandler backgroundEventHandler = new(presenterFacade, backgroundRepository);
            AnimationEventHandler animationEventHandler = new(presenterFacade, animationRepository);
            PortraitEventHandler portraitEventHandler = new(presenterFacade, portraitRepository);
            LayerEventHandler layerEventHandler = new(presenterFacade);
            handlerRepo.Register<TextEvent>(textEventHandler.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandler.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandler.HandleAsync);
            handlerRepo.Register<AnimationEventData>(animationEventHandler.HandleAsync);
            handlerRepo.Register<PortraitEvent>(portraitEventHandler.HandleAsync);
            handlerRepo.Register<LayerEvent>(layerEventHandler.HandleAsync);

            List<string> layerOrder = new(_loadedScenarioSettings.LayerBackToFront.Count);
            for (int i = 0; i < _loadedScenarioSettings.LayerBackToFront.Count; i++)
            {
                layerOrder.Add(_loadedScenarioSettings.LayerBackToFront[i].ToString());
            }

            _scenarioView.Initialize(
                _scenarioViewModel,
                BuildBackgroundMap(_loadedBackgroundCatalog),
                BuildAnimationMap(_loadedAnimationCatalog),
                BuildPortraitMap(_loadedPortraitCatalog),
                layerOrder);
            SetScenarioDisplayActive(false);
            return true;
        }

        /// <summary>
        ///     Missionとシナリオ再生を結合します。
        /// </summary>
        /// <returns>結合に成功した場合はtrueです。</returns>
        private bool TryInitializeMissionScenarioController()
        {
            if (!ServiceLocator.TryGetInstance(out InputComposition inputComposition)
                || inputComposition.GetInputView == null
                || inputComposition.GetInputMapController == null
                || !ServiceLocator.TryGetInstance(out SequenceModuleContainer sequenceModuleContainer)
                || sequenceModuleContainer.BattlePauseController == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] シナリオ再生に必要な入力または戦闘ポーズのモジュールを取得できませんでした。", this);
                return false;
            }

            _scenarioInputView.Initialize(_scenarioInputController, inputComposition.GetInputView);
            _scenarioController = new MissionScenarioController(
                _moduleContainer.MissionRuntimeService,
                _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                _scenarioUsecase,
                sequenceModuleContainer.BattlePauseController,
                new InGameScenarioInputModeController(inputComposition.GetInputMapController));
            _scenarioController.OnScenarioPlaybackStarted += HandleScenarioPlaybackStarted;
            _scenarioController.OnScenarioPlaybackEnded += HandleScenarioPlaybackEnded;
            _scenarioController.Start();
            return true;
        }

        /// <summary>
        ///     シナリオ再生開始時に表示を有効化します。
        /// </summary>
        private void HandleScenarioPlaybackStarted()
        {
            SetScenarioDisplayActive(true);
        }

        /// <summary>
        ///     シナリオ再生終了時に表示を無効化します。
        /// </summary>
        private void HandleScenarioPlaybackEnded()
        {
            SetScenarioDisplayActive(false);
        }

        /// <summary>
        ///     シナリオ表示用のルートを有効または無効にします。
        /// </summary>
        /// <param name="isActive">有効にする場合はtrueです。</param>
        private void SetScenarioDisplayActive(bool isActive)
        {
            if (_scenarioRoot != null && _scenarioRoot.activeSelf != isActive)
            {
                _scenarioRoot.SetActive(isActive);
                // ScenarioViewは自身を非Activateにするため、ここでActiveにする
                _scenarioView.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        ///     シナリオ再生で使用するInspector参照を検証します。
        /// </summary>
        /// <returns>参照が有効な場合はtrueです。</returns>
        private bool ValidateScenarioPlaybackReferences()
        {
            if (_scenarioRoot == gameObject
                || _scenarioRoot == null
                || _scenarioView == null
                || _scenarioInputView == null
                || string.IsNullOrWhiteSpace(_backgroundCatalogKey)
                || string.IsNullOrWhiteSpace(_animationCatalogKey)
                || string.IsNullOrWhiteSpace(_portraitCatalogKey)
                || string.IsNullOrWhiteSpace(_scenarioSettingsKey))
            {
                Debug.LogError($"[{nameof(InGameMissionInitializer)}] シナリオ表示関連の参照か、Addressablesキーの設定が不足です。", this);
                return false;
            }
            return true;
        }

        /// <summary>
        ///     背景アセット参照用の辞書を構築します。
        /// </summary>
        /// <param name="catalog">背景カタログです。</param>
        /// <returns>アセットキーをキーにした背景一覧です。</returns>
        private static IReadOnlyDictionary<string, Sprite> BuildBackgroundMap(BackgroundCatalogAsset catalog)
        {
            Dictionary<string, Sprite> map = new(StringComparer.Ordinal);
            if (catalog == null)
            {
                return map;
            }

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                BackgroundCatalogEntry entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null)
                {
                    continue;
                }

                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        ///     アニメーションアセット参照用の辞書を構築します。
        /// </summary>
        /// <param name="catalog">アニメーションカタログです。</param>
        /// <returns>アセットキーをキーにしたアニメーション一覧です。</returns>
        private static IReadOnlyDictionary<string, AnimationClip> BuildAnimationMap(AnimationCatalogAsset catalog)
        {
            Dictionary<string, AnimationClip> map = new(StringComparer.Ordinal);
            if (catalog == null)
            {
                return map;
            }

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                AnimationCatalogEntry entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null)
                {
                    continue;
                }

                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        ///     立ち絵アセット参照用の辞書を構築します。
        /// </summary>
        /// <param name="catalog">立ち絵カタログです。</param>
        /// <returns>アセットキーをキーにした立ち絵一覧です。</returns>
        private static IReadOnlyDictionary<string, Sprite> BuildPortraitMap(PortraitCatalogAsset catalog)
        {
            Dictionary<string, Sprite> map = new(StringComparer.Ordinal);
            if (catalog == null)
            {
                return map;
            }

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                PortraitCatalogEntry entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null)
                {
                    continue;
                }

                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        private void OnDestroy()
        {
            if (_registeredMissionRuntimeService)
            {
                ServiceLocator.UnregisterInstance<MissionRuntimeService>();
                _registeredMissionRuntimeService = false;
            }

            if (_registeredMissionEventController)
            {
                ServiceLocator.UnregisterInstance<MissionEventController>();
                _registeredMissionEventController = false;
            }
        }

        /// <summary>
        ///     Inspector参照を検証します。
        /// </summary>
        /// <returns> 参照が有効な場合はtrue。 </returns>
        private bool ValidateReferences()
        {
            if (_missionHudView == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(_missionHudView)}が設定されていません。",
                    this);

                return false;
            }

            if (_missionLoopView == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(_missionLoopView)}が設定されていません。",
                    this);

                return false;
            }
            if(_comboHudView == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(_comboHudView)}が設定されていません。",
                    this);

                return false;
            }

            return true;
        }
    }
}
