using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;
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
                _loadedScreenRuleData = await _screenRuleDataKey.LoadAssetAsync<ScreenRuleData>(this, destroyCancellationToken);
            }
            catch (Exception ex) { Debug.LogException(ex, this); }
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

            _screenController.ShowHome();
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
            _isSubscribed = false;

            ServiceLocator.UnregisterInstance<SkillBuildScreenView>();

            _screenViewRegistry?.Dispose();
            _screenViewRegistry = null;

            CancelAndDispose(ref _ctsTransition);
            _screenRuleDataKey.ReleaseLoadedAsset(this);
            _loadedScreenRuleData = null;
            _isInitialized = false;
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
            StageSelectScreenView stageSelectScreenView = new StageSelectScreenView(stageSelectRoot, _outGameUIEvent);
            SkillTreeScreenView skillTreeScreenView = new SkillTreeScreenView(skillTreeRoot, _outGameUIEvent);
            PlayerStatusScreenView playerStatusScreenView = new PlayerStatusScreenView(playerStatusRoot, _outGameUIEvent);
            SkillBuildScreenView skillBuildScreenView = new SkillBuildScreenView(skillBuildRoot, _outGameUIEvent);
            BattlePreparationScreen battlePreparationScreen = new BattlePreparationScreen(battlePreparationRoot, _outGameUIEvent);
            SettingScreenView settingScreenView = new SettingScreenView(settingRoot, _outGameUIEvent);

            // SkillBuild 専用 Initializer から取得できるように登録する。
            ServiceLocator.RegisterInstance(skillBuildScreenView);

            ScreenViewRegistry screenViewRegistry = new(
                homeScreenView,
                stageSelectScreenView,
                skillTreeScreenView,
                skillBuildScreenView,
                battlePreparationScreen,
                settingScreenView);

            _screenViewRegistry = screenViewRegistry;
            screenViewRegistry.HideAll();

            // InfraStructure 層
            IScreenStateRepository screenStateRepository = new ScreenStateRepository();
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

            _ctsTransition = new();

            _isInitialized = true;
            _isStartGame = false;
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
            _outGameUIEvent.OnOutGameUiVisibilityChanged -= HandleOutGameUiVisibilityChanged;
            _isSubscribed = false;
        }

        /// <summary>
        ///     ホーム画面表示イベントを処理します。
        /// </summary>
        private void HandleHomeScreenShown()
        {
            _screenController.ShowHome();
        }

        /// <summary>
        ///     ステージ選択表示イベントを処理します。
        ///     画面表示後に OnStageSelectScreenCompleted を発火します。
        /// </summary>
        private void HandleStageSelectionScreenShown()
        {
            _screenController.ShowStageSelect();

            // 作戦画面の表示完了を通知する
            _outGameUIEvent.OnStageSelectScreenCompleted?.Invoke();
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
        private void HandleBattlePreparationScreenShown(string targetSceneName)
        {
            _screenController.ShowBattlePreparation(targetSceneName);
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
        /// <param name="targetSceneName"> 遷移先のシーン名。 </param>
        private async void HandleStartGame(string targetSceneName)
        {
            // 一度ゲーム開始処理が走った後は、二重に処理が走らないようにします。
            if (_isStartGame) { return; }

            _isStartGame = true;
            var currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            try
            {
                bool success =
                    await _sceneTransitionController
                        .ChangeSceneKeepingLoadingAsync(
                            currentSceneName,
                            targetSceneName,
                            _ctsTransition.Token);

                if (success)
                {
                    return;
                }

                _isStartGame = false;

                Debug.LogError(
                    $"[{nameof(ScreenInitializer)}] " +
                    "インゲームシーンへの遷移に失敗しました。" +
                    $" SceneName: {targetSceneName}",
                    this);
            }
            catch (OperationCanceledException)
            {
                _isStartGame = false;
            }
            catch (Exception exception)
            {
                _isStartGame = false;
                Debug.LogException(exception, this);
            }
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

        /// <summary>
        ///     CancellationTokenSource をキャンセルし、破棄します。
        ///     さらに、参照を null に設定します。
        /// </summary>
        private void CancelAndDispose(ref CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
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
        [SerializeField, SourceDataAddress, Tooltip("画面遷移ルールデータの Addressables キーです。")]
        private string _screenRuleDataKey;

        private ScreenController _screenController;
        private OutGameUIEvent _outGameUIEvent;
        private ScreenViewRegistry _screenViewRegistry;
        private SceneTransitionController _sceneTransitionController;
        private ScreenRuleData _loadedScreenRuleData;
        private bool _isInitialized = false;
        private bool _isSubscribed;
        private bool _isStartGame = false;

        private CancellationTokenSource _ctsTransition;
    }
}
