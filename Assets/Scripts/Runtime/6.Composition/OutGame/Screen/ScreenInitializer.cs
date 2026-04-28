using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
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

            _screenController.ShowHome();
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
            _screenController.ShowHome();
        }

        /// <summary>
        ///     ステージ選択表示イベントを処理します。
        /// </summary>
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
        ///     画面クローズイベントを処理します。
        /// </summary>
        private void HandleScreenClosed()
        {
            _screenController.CloseCurrent();
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

        private ScreenController _screenController;
        private OutGameUIEvent _outGameUIEvent;
        private ScreenViewRegistry _screenViewRegistry;
        private bool _isInitialized = false;
    }
}
