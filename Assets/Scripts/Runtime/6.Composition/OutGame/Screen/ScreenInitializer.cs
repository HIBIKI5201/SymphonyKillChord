using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     アウトゲーム画面の依存を解決するクラス。
    /// </summary>
    public sealed class ScreenInitializer : MonoBehaviour
    {
        /// <summary>
        ///     初期化を行います。
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        ///     イベント購読を行います。
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        ///     イベント購読解除を行います。
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
            _screenViewRegistry?.Dispose();

            CancelAndDispose(ref _ctsShow);
            CancelAndDispose(ref _ctsHide);
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        private void Initialize()
        {
            _outGameUIEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
            if (_outGameUIEvent == null)
            {
                Debug.LogError($"[{nameof(ScreenInitializer)}] OutGameUIEvent が取得できませんでした.", this);
                return;
            }

            if (_uiDocument == null)
            {
                Debug.LogError($"[{nameof(ScreenInitializer)}] UIDocument が設定されていません。", this);
                return;
            }

            // View 層
            VisualElement rootElement = _uiDocument.rootVisualElement;

            HomeScreenView homeScreenView = new(rootElement.Q<VisualElement>(HOMESCREEN_NAME), _outGameUIEvent);
            StageSelectScreenView stageSelectScreenView = new(rootElement.Q<VisualElement>(STAGESELECTSCREEN_NAME), _outGameUIEvent);
            SkillTreeScreenView skillTreeScreenView = new(rootElement.Q<VisualElement>(SKILLTREESCREEN_NAME), _outGameUIEvent);
            SkillBuildScreenView skillBuildScreenView = new(rootElement.Q<VisualElement>(SKILLBUILDSCREEN_NAME), _outGameUIEvent);
            SettingScreenView settingScreenView = new(rootElement.Q<VisualElement>(SETTINGSCREEN_NAME), _outGameUIEvent);

            ScreenViewRegistry screenViewRegistry = new(
                homeScreenView,
                stageSelectScreenView,
                skillTreeScreenView,
                skillBuildScreenView,
                settingScreenView);

            _screenViewRegistry = screenViewRegistry;
            screenViewRegistry.HideAllImmediately();

            // InfraStructure 層
            IScreenStateRepository screenStateRepository = new ScreenStateRepository();
            IScreenRuleRepository screenRuleRepository = new ScreenRuleRepository(_screenRuleData);

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

            _ctsShow = new();
            _ctsHide = new();

            _ = _screenController.ShowHome(_ctsShow.Token);
            _isInitialized = true;
        }

        /// <summary>
        ///     UI イベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized) { return; }
            _outGameUIEvent.OnShownHomeScreen += HandleHomeScreenShown;
            _outGameUIEvent.OnShownStageSelectionScreen += HandleStageSelectionScreenShown;
            _outGameUIEvent.OnShownSkillTreeScreen += HandleSkillTreeScreenShown;
            _outGameUIEvent.OnShownSkillBuildScreen += HandleSkillBuildScreenShown;
            _outGameUIEvent.OnShownSettingScreen += HandleSettingsShown;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
        }

        /// <summary>
        ///     UI イベント購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isInitialized) { return; }
            _outGameUIEvent.OnShownHomeScreen -= HandleHomeScreenShown;
            _outGameUIEvent.OnShownStageSelectionScreen -= HandleStageSelectionScreenShown;
            _outGameUIEvent.OnShownSkillTreeScreen -= HandleSkillTreeScreenShown;
            _outGameUIEvent.OnShownSkillBuildScreen -= HandleSkillBuildScreenShown;
            _outGameUIEvent.OnShownSettingScreen -= HandleSettingsShown;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;

            _outGameUIEvent.UnregisterOutGameUIEvent();
        }

        /// <summary>
        ///     ホーム画面表示イベントを処理します。
        /// </summary>
        private void HandleHomeScreenShown()
        {
            // 前回の画面の表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.ShowHome(RenewShowToken());
        }

        /// <summary>
        ///     ステージ選択表示イベントを処理します。
        /// </summary>
        private void HandleStageSelectionScreenShown()
        {
            // 前回の画面の表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.ShowStageSelect(RenewShowToken());
        }

        /// <summary>
        ///     スキルツリー表示イベントを処理します。
        /// </summary>
        private void HandleSkillTreeScreenShown()
        {
            // 前回の画面の表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.ShowSkillTree(RenewShowToken());
        }

        /// <summary>
        ///     スキル選択表示イベントを処理します。
        /// </summary>
        private void HandleSkillBuildScreenShown()
        {
            // 前回の画面の表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.ShowSkillBuild(RenewShowToken());
        }

        /// <summary>
        ///     設定表示イベントを処理します。
        /// </summary>
        private void HandleSettingsShown()
        {
            // 前回の画面の表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.ShowSetting(RenewShowToken());
        }

        /// <summary>
        ///     画面クローズイベントを処理します。
        /// </summary>
        private void HandleScreenClosed()
        {
            // 前回の画面の非表示が完了していない場合は、完了するまで待機します。
            if (IsTransitioning) { return; }

            _transitionTask = _screenController.CloseCurrent(RenewHideToken());
        }

        /// <summary>
        ///     Task のキャンセルと新しい CancellationTokenSource の生成を行います。
        /// </summary>
        /// <returns> 新しい CancellationToken を返します。 </returns>
        private CancellationToken RenewShowToken()
        {
            CancelAndDispose(ref _ctsShow);
            _ctsShow = new();
            return _ctsShow.Token;
        }

        /// <summary>
        ///     Task のキャンセルと新しい CancellationTokenSource の生成を行います。
        /// </summary>
        /// <returns> 新しい CancellationToken を返します。 </returns>
        private CancellationToken RenewHideToken()
        {
            CancelAndDispose(ref _ctsHide);
            _ctsHide = new();
            return _ctsHide.Token;
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
        private const string SKILLBUILDSCREEN_NAME = "SkillBuildContainer";
        private const string SETTINGSCREEN_NAME = "SettingContainer";

        [SerializeField]
        [Tooltip("画面表示に使用する UIDocument です。")]
        private UIDocument _uiDocument;
        [SerializeField, Tooltip("画面遷移ルールデータです。")]
        private ScreenRuleData _screenRuleData;
        private bool IsTransitioning => _transitionTask != null && !_transitionTask.IsCompleted;

        private ScreenController _screenController;
        private OutGameUIEvent _outGameUIEvent;
        private ScreenViewRegistry _screenViewRegistry;
        private bool _isInitialized = false;

        private Task _transitionTask;

        private CancellationTokenSource _ctsShow;
        private CancellationTokenSource _ctsHide;
    }
}
