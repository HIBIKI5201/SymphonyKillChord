using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     アウトゲーム画面の依存を解決するクラス。
    /// </summary>
    public sealed class ScreenInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(ScreenInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 100;

        /// <summary>
        ///     画面遷移ルールデータをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            try
            {
                _loadedScreenRuleData = await _screenRuleDataKey.LoadAssetAsync<ScreenRuleData>(this, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }

            return _loadedScreenRuleData != null;
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            return Initialize();
        }

        /// <summary>
        ///     他モジュールとの結合を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            Subscribe();
            if (!_isInitialized)
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _loadingScreenController))
            {
                Debug.LogError($"[{nameof(ScreenInitializer)}] LoadingScreenControllerを取得できませんでした。", this);
                return false;
            }

            if (!_isLoadingSubscribed)
            {
                _loadingScreenController.LoadingStarted += HandleLoadingStarted;
                _loadingScreenController.LoadingCompleted += HandleLoadingCompleted;
                _isLoadingSubscribed = true;
            }

            if (!_isOptionInputSubscribed)
            {
                if (ServiceLocator.TryGetInstance(out _playerInputView))
                {
                    _playerInputView.OnOptionInput += HandleOptionInputHandler;
                    _isOptionInputSubscribed = true;
                }
                else
                {
                    Debug.LogWarning(
                        $"[{nameof(ScreenInitializer)}] PlayerInputViewを取得できませんでした。"
                        + "コントローラーのOptionボタンでの設定画面表示は無効になります。",
                        this);
                }
            }

            if (!ServiceLocator.TryGetInstance(out InputComposition inputComposition))
            {
                Debug.LogError($"[{nameof(ScreenInitializer)}] InputCompositionを取得できませんでした。", this);
                return false;
            }

            // ホーム画面初回表示時点でOutGame入力マップ(Submit/Cancel)を有効化する。
            // 出撃/シナリオ復帰時のみ有効化されていたため、それらを経由しない初回起動時は
            // コントローラー/キーボードのCancel(Bボタン/Esc)が機能しなかった。
            inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);

            ApplyInteractionEnabled(!_loadingScreenController.IsLoading);
            if (!SaveStore.IsLoaded<SaveData>()
                || SaveStore.Get<SaveData>().Tutorial.Phase >= TutorialPhase.BattleCompleted)
            {
                _screenController.ShowHome();
            }
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            UnsubscribeLoading();
            UnsubscribeOptionInput();
            Unsubscribe();
            _isSubscribed = false;

            ServiceLocator.UnregisterInstance<SkillBuildScreenView>();
            ServiceLocator.UnregisterInstance<BattlePreparationScreen>();
            ServiceLocator.UnregisterInstance<HomeScreenView>();
            ServiceLocator.UnregisterInstance<SettingScreenView>();
            _screenViewRegistry?.Dispose();
            _screenViewRegistry = null;
            _screenStateRepository = null;

            _screenRuleDataKey.ReleaseLoadedAsset(this);
            _loadedScreenRuleData = null;
            _isInitialized = false;
        }

        /// <summary>
        ///     ロード開始時に登録画面の操作を禁止する。
        /// </summary>
        private void HandleLoadingStarted()
        {
            ApplyInteractionEnabled(false);
        }

        /// <summary>
        ///     ロード終了時は成否にかかわらず画面操作と現在画面のフォーカスを復元する。
        /// </summary>
        private void HandleLoadingCompleted(bool success)
        {
            ApplyInteractionEnabled(true);
        }

        /// <summary>
        ///     取得済みRegistryへ入力許可を適用し、例外で他のロード購読者の通知を中断させない。
        /// </summary>
        private void ApplyInteractionEnabled(bool isEnabled)
        {
            try
            {
                _screenViewRegistry?.SetInteractionEnabled(isEnabled);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     View破棄より先にロード通知の購読を一度だけ解除する。
        /// </summary>
        private void UnsubscribeLoading()
        {
            if (_isLoadingSubscribed && _loadingScreenController != null)
            {
                _loadingScreenController.LoadingStarted -= HandleLoadingStarted;
                _loadingScreenController.LoadingCompleted -= HandleLoadingCompleted;
            }

            _isLoadingSubscribed = false;
            _loadingScreenController = null;
        }

        /// <summary>
        ///     View破棄より先にコントローラーOption入力の購読を一度だけ解除する。
        /// </summary>
        private void UnsubscribeOptionInput()
        {
            if (_isOptionInputSubscribed && _playerInputView != null)
            {
                _playerInputView.OnOptionInput -= HandleOptionInputHandler;
            }

            _isOptionInputSubscribed = false;
            _playerInputView = null;
        }

        /// <summary>
        ///     コントローラーのOptionボタン(Xboxの≡/PlayStationのOptions)で設定画面を開く。
        /// </summary>
        /// <param name="inputContext"> 入力情報。 </param>
        private void HandleOptionInputHandler(InputContext<float> inputContext)
        {
            // 押した瞬間のみ反応させる。離した際の通知では開かない。
            if (!_isInitialized || inputContext.Phase != InputActionPhase.Performed)
            {
                return;
            }

            // 既に設定画面を表示中の場合、連打で遷移履歴に同じ画面が積み重なってしまうため
            // 何もしない。
            if (_screenStateRepository != null
                && _screenStateRepository.TransitionState.CurrentScreenId == ScreenId.Setting)
            {
                return;
            }

            _outGameUIEvent.OnShownSettingScreen?.Invoke();
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        private bool Initialize()
        {
            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] OutGameUIEvent が取得できませんでした.", this);
#endif
                return false;
            }

            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] UIDocument が設定されていません。", this);
#endif
                return false;
            }
            if (_loadedScreenRuleData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] ScreenRuleData が設定されていません。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _sceneTransitionController))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] SceneTransitionController が取得できませんでした.", this);
#endif
                return false;
            }

            // View 層
            VisualElement rootElement = _uiDocument.rootVisualElement;

            VisualElement homeRoot = rootElement.Q<VisualElement>(HOMESCREEN_NAME);
            VisualElement stageSelectRoot = rootElement.Q<VisualElement>(STAGESELECTSCREEN_NAME);
            VisualElement skillTreeRoot = rootElement.Q<VisualElement>(SKILLTREESCREEN_NAME);
            VisualElement playerStatusRoot = skillTreeRoot.Q<VisualElement>(SKILLTREESCREEN_PLAYERSTATUS_NAME);
            VisualElement skillBuildRoot = rootElement.Q<VisualElement>(SKILLBUILDSCREEN_NAME);
            VisualElement battlePreparationRoot = rootElement.Q<VisualElement>(BATTLEPREPARATIONSCREEN_NAME);
            VisualElement settingRoot = rootElement.Q<VisualElement>(SETTINGSCREEN_NAME);

            // 各画面のルート要素が見つからない場合は、エラーログを出力して初期化を中断します。
            if (homeRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {HOMESCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (stageSelectRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {STAGESELECTSCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (skillTreeRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {SKILLTREESCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (playerStatusRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {SKILLTREESCREEN_PLAYERSTATUS_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (skillBuildRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {SKILLBUILDSCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (battlePreparationRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {BATTLEPREPARATIONSCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }
            if (settingRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(ScreenInitializer)}] {SETTINGSCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }

            HomeScreenView homeScreenView = new HomeScreenView(homeRoot, _outGameUIEvent);
            _homeScreenView = homeScreenView;
            _getHomePointsUseCase = new GetHomePointsUseCase();
            StageSelectScreenView stageSelectScreenView = new StageSelectScreenView(stageSelectRoot, _outGameUIEvent);
            SkillTreeScreenView skillTreeScreenView = new SkillTreeScreenView(skillTreeRoot, _outGameUIEvent);
            PlayerStatusScreenView playerStatusScreenView = new PlayerStatusScreenView(playerStatusRoot, _outGameUIEvent, null, null, null, null, null);
            SkillBuildScreenView skillBuildScreenView = new SkillBuildScreenView(skillBuildRoot, _outGameUIEvent, _comboHexIcon);
            BattlePreparationScreen battlePreparationScreen = new BattlePreparationScreen(battlePreparationRoot, _outGameUIEvent);
            SettingScreenView settingScreenView = new SettingScreenView(settingRoot, _outGameUIEvent);

            // SkillBuild 専用 Initializer から取得できるように登録する。
            ServiceLocator.RegisterInstance(skillBuildScreenView);
            ServiceLocator.RegisterInstance(battlePreparationScreen);
            // HomeCharacterPreviewInitializer から取得できるように登録する。
            ServiceLocator.RegisterInstance(homeScreenView);
            // SettingComposition から取得できるように登録する。
            ServiceLocator.RegisterInstance(settingScreenView);

            ScreenViewRegistry screenViewRegistry = new(
                homeScreenView,
                stageSelectScreenView,
                skillTreeScreenView,
                skillBuildScreenView,
                battlePreparationScreen,
                settingScreenView);

            _screenViewRegistry = screenViewRegistry;
            screenViewRegistry.HideAllImmediately();

            // InfraStructure 層
            IScreenStateRepository screenStateRepository = new ScreenStateRepository();
            _screenStateRepository = screenStateRepository;
            IScreenRuleRepository screenRuleRepository = new ScreenRuleRepository(_loadedScreenRuleData);

            //  Adaptor 層
            IScreenTransitionApplicable screenViewModel = new ScreenViewApplicator(screenViewRegistry);
            IScreenPresenter screenPresenter = new ScreenPresenter(screenViewModel);

            // Application 層
            ShowScreenUseCase showScreenUseCase = new(
                screenStateRepository,
                screenRuleRepository,
                screenPresenter);

            CloseCurrentScreenUseCase closeCurrentScreenUseCase = new(
                screenStateRepository,
                screenPresenter);

            ResetToHomeScreenUseCase resetToHomeScreenUseCase = new(
                screenStateRepository,
                screenPresenter);

            _screenController = new(
                showScreenUseCase,
                closeCurrentScreenUseCase,
                resetToHomeScreenUseCase);

            _isInitialized = true;
            _isSceneTransitioning = false;
            return true;
        }

        /// <summary>
        ///     UI イベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || _isSubscribed) { return; }
            _outGameUIEvent.OnShownHomeScreen += HandleHomeScreenShown;
            _outGameUIEvent.OnShownStageSelectionScreen += HandleStageSelectionScreenShown;
            _outGameUIEvent.OnShownSkillTreeScreen += HandleSkillTreeScreenShown;
            _outGameUIEvent.OnShownSkillBuildScreen += HandleSkillBuildScreenShown;
            _outGameUIEvent.OnShownBattlePreparationScreen += HandleBattlePreparationScreenShown;
            _outGameUIEvent.OnShownSettingScreen += HandleSettingsShown;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
            _outGameUIEvent.OnStartGame += HandleStartGame;
            _outGameUIEvent.OnReturnToTitleRequested += HandleReturnToTitleRequested;
            _outGameUIEvent.OnOutGameUiVisibilityChanged += HandleOutGameUiVisibilityChanged;
            _isSubscribed = true;
        }

        /// <summary>
        ///     UI イベント購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || !_isSubscribed) { return; }
            _outGameUIEvent.OnShownHomeScreen -= HandleHomeScreenShown;
            _outGameUIEvent.OnShownStageSelectionScreen -= HandleStageSelectionScreenShown;
            _outGameUIEvent.OnShownSkillTreeScreen -= HandleSkillTreeScreenShown;
            _outGameUIEvent.OnShownSkillBuildScreen -= HandleSkillBuildScreenShown;
            _outGameUIEvent.OnShownBattlePreparationScreen -= HandleBattlePreparationScreenShown;
            _outGameUIEvent.OnShownSettingScreen -= HandleSettingsShown;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;
            _outGameUIEvent.OnStartGame -= HandleStartGame;
            _outGameUIEvent.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
            _outGameUIEvent.OnOutGameUiVisibilityChanged -= HandleOutGameUiVisibilityChanged;
            _isSubscribed = false;
        }

        /// <summary>
        ///     ホーム画面表示イベントを処理します。
        /// </summary>
        private void HandleHomeScreenShown()
        {
            VisualElement rootElement = _uiDocument?.rootVisualElement;
            if (rootElement != null
                && rootElement.resolvedStyle.display == DisplayStyle.None)
            {
                // OutGame UIが非表示なら、復帰前の画面を描画せず即時に破棄する。
                _screenViewRegistry.HideAllImmediately();
            }

            _screenController.ShowHome();
            RefreshHomePointsAsync();
        }

        /// <summary>
        ///     ホーム画面のトップバーに表示するポイントを最新の状態へ更新します。
        /// </summary>
        private async void RefreshHomePointsAsync()
        {
            HomePoints points = await _getHomePointsUseCase.ExecuteAsync();
            _homeScreenView?.SetPoints(points.RebuildPoints, points.UnlockPoints);
        }

        /// <summary>
        ///     ステージ選択表示イベントを処理します。
        /// </summary>
        /// <remarks>
        ///     OnStageSelectScreenCompleted はフェード完了のタイミングで StageSelectScreenView 自身が発火します。
        /// </remarks>
        private void HandleStageSelectionScreenShown()
        {
            _screenController.ShowStageSelect();
        }

        /// <summary>
        ///     スキルツリー表示イベントを処理します。
        /// </summary>
        private void HandleSkillTreeScreenShown()
        {
            _screenController.ShowSkillTree();
        }

        /// <summary>
        ///     スキル選択表示イベントを処理します。
        /// </summary>
        private void HandleSkillBuildScreenShown()
        {
            _screenController.ShowSkillBuild();
        }

        /// <summary>
        ///     設定表示イベントを処理します。
        /// </summary>
        private void HandleSettingsShown()
        {
            _screenController.ShowSetting();
        }

        /// <summary>
        ///     戦闘準備画面表示イベントを処理します。
        /// </summary>
        private void HandleBattlePreparationScreenShown()
        {
            _screenController.ShowBattlePreparation();
        }

        /// <summary>
        ///     画面クローズイベントを処理します。
        /// </summary>
        private void HandleScreenClosed()
        {
            _screenController.CloseCurrent();
        }

        /// <summary>
        ///     インゲームへの遷移イベントを処理します。
        /// </summary>
        private async void HandleStartGame()
        {
            // 一度ゲーム開始処理が走った後は、二重に処理が走らないようにします。
            if (_isSceneTransitioning || _sceneTransitionController.HasScenarioBattleSortie
                || _sceneTransitionController.PersistentLifetimeToken.IsCancellationRequested) { return; }

            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState)
                || !selectedBattleStageState.HasSelectedBattleStage
                || string.IsNullOrWhiteSpace(selectedBattleStageState.InGameSceneName))
            {
                Debug.LogError(
                    $"[{nameof(ScreenInitializer)}] 出撃対象のバトルステージが選択されていません。",
                    this);
                return;
            }

            string targetSceneName = selectedBattleStageState.InGameSceneName;
            _isSceneTransitioning = true;
            string currentSceneName = gameObject.scene.name;
            try
            {
                bool success =
                    await _sceneTransitionController
                        .ChangeSceneKeepingLoadingWithPersistentLifetimeAsync(
                            currentSceneName,
                            targetSceneName);

                if (success)
                {
                    return;
                }

                if (this == null || _sceneTransitionController.PersistentLifetimeToken.IsCancellationRequested) { return; }
                _isSceneTransitioning = false;

                Debug.LogError(
                    $"[{nameof(ScreenInitializer)}] " +
                    "インゲームシーンへの遷移に失敗しました。" +
                    $" SceneName: {targetSceneName}",
                    this);
            }
            catch (OperationCanceledException)
            {
                if (this != null) { _isSceneTransitioning = false; }
            }
            catch (Exception exception)
            {
                if (this != null) { _isSceneTransitioning = false; }
                Debug.LogException(exception);
            }
        }

        /// <summary>
        ///     OutGameからタイトル画面へ遷移する。
        /// </summary>
        private async void HandleReturnToTitleRequested()
        {
            if (_isSceneTransitioning || _sceneTransitionController.HasScenarioBattleSortie
                || _sceneTransitionController.PersistentLifetimeToken.IsCancellationRequested)
            {
                _outGameUIEvent.OnReturnToTitleRequestCompleted?.Invoke(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(_titleSceneName))
            {
                Debug.LogError(
                    $"[{nameof(ScreenInitializer)}] タイトルシーン名が設定されていません。",
                    this);
                _outGameUIEvent.OnReturnToTitleRequestCompleted?.Invoke(false);
                return;
            }

            _isSceneTransitioning = true;
            string currentSceneName = gameObject.scene.name;
            string titleSceneName = _titleSceneName;
            OutGameUIEvent uiEvent = _outGameUIEvent;
            SceneTransitionController transition = _sceneTransitionController;

            try
            {
                bool success = await transition.ChangeSceneWithPersistentLifetimeAsync(
                    currentSceneName, titleSceneName);
                if (success)
                {
                    if (!transition.PersistentLifetimeToken.IsCancellationRequested)
                    {
                        uiEvent.OnReturnToTitleRequestCompleted?.Invoke(true);
                    }
                    return;
                }

                Debug.LogError(
                    $"[{nameof(ScreenInitializer)}] タイトル画面への遷移に失敗しました。"
                    + $" SceneName: {titleSceneName}");
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (this == null || transition.PersistentLifetimeToken.IsCancellationRequested) { return; }
            _isSceneTransitioning = false;
            uiEvent.OnReturnToTitleRequestCompleted?.Invoke(false);
        }

        /// <summary>
        ///     OutGame UIの表示状態変更イベントを処理します。
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue。</param>
        private void HandleOutGameUiVisibilityChanged(bool isVisible)
        {
            if (_uiDocument == null)
            {
                return;
            }

            VisualElement rootElement = _uiDocument.rootVisualElement;
            if (rootElement == null)
            {
                return;
            }

            rootElement.style.display = isVisible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private const string HOMESCREEN_NAME = "HomeContainer";
        private const string STAGESELECTSCREEN_NAME = "StageSelectContainer";
        private const string SKILLTREESCREEN_NAME = "SkillTreeContainer";
        private const string SKILLTREESCREEN_PLAYERSTATUS_NAME = "PlayerStatus";
        private const string SKILLBUILDSCREEN_NAME = "SkillBuildContainer";
        private const string BATTLEPREPARATIONSCREEN_NAME = "BattlePreparationContainer";
        private const string SETTINGSCREEN_NAME = "SettingContainer";

        [SerializeField]
        [Tooltip("画面表示に使用する UIDocument です。")]
        private UIDocument _uiDocument;

        [SerializeField]
        [Tooltip("発動コマンド表示に使う正六角形スプライト（Assets/Arts/UI/UI_hexagon.png）です。")]
        private Sprite _comboHexIcon;
        [SerializeField, SourceDataAddress, Tooltip("画面遷移ルールデータの Addressables キーです。")]
        private string _screenRuleDataKey;
        [SerializeField, SceneNameSelector, Tooltip("設定画面から戻るタイトルシーン名です。")]
        private string _titleSceneName = "Title";

        private ScreenController _screenController;
        private OutGameUIEvent _outGameUIEvent;
        private ScreenViewRegistry _screenViewRegistry;
        private SceneTransitionController _sceneTransitionController;
        private ScreenRuleData _loadedScreenRuleData;
        private HomeScreenView _homeScreenView;
        private GetHomePointsUseCase _getHomePointsUseCase;
        private bool _isInitialized = false;
        private bool _isSubscribed;
        private bool _isLoadingSubscribed;
        private LoadingScreenController _loadingScreenController;
        private bool _isOptionInputSubscribed;
        private PlayerInputView _playerInputView;
        private IScreenStateRepository _screenStateRepository;
        private bool _isSceneTransitioning = false;

    }
}
