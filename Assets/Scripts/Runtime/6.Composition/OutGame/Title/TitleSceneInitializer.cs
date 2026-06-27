using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Title;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの初期化を行うクラス。
    /// </summary>
    public class TitleSceneInitializer : MonoBehaviour
    {
        private const string TITLE_SCREEN_NAME = "TitleContainer";
        private const string MENU_SCREEN_NAME = "MenuContainer";
        private const string OPTION_SCREEN_NAME = "OptionContainer";
        private const string CREDIT_SCREEN_NAME = "CreditContainer";

        [SerializeField, Tooltip("UI Document")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("画面遷移ルールデータ")]
        private ScreenRuleData _ruleData;

        [Header("シーン遷移設定")]
        [SerializeField, SceneNameSelector, Tooltip("遷移元のシーン名")]
        private string _currentSceneName;

        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名")]
        private string _targetSceneName;

        [SerializeField, SceneNameSelector, Tooltip("初回起動時の遷移先のシーン名")]
        private string _firstLaunchTargetSceneName;

        private SaveData _saveData;

        private OutGameUIEvent _outGameUIEvent;
        private TitleScreenViewRegistry _titleScreenViewRegistry;
        private ScreenController _screenController;

        private async void Start()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: UI Documentが設定されていません。");
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionController sceneTransitionController))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SceneTransitionControllerがServiceLocatorに登録されていません。");
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: OutGameUIEventがServiceLocatorに登録されていません。");
#endif
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: Root VisualElementがnullです。");
#endif
                return;
            }

            TitleStartController titleStartController = new(sceneTransitionController);

            var titleRoot = root.Q<VisualElement>(TITLE_SCREEN_NAME);
            var menuRoot = root.Q<VisualElement>(MENU_SCREEN_NAME);
            var optionRoot = root.Q<VisualElement>(OPTION_SCREEN_NAME);
            var creditRoot = root.Q<VisualElement>(CREDIT_SCREEN_NAME);
            if (titleRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: タイトル画面のルートVisualElementが見つかりません。{TITLE_SCREEN_NAME}");
#endif
                return;
            }

            if (menuRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: メニュー画面のルートVisualElementが見つかりません。{MENU_SCREEN_NAME}");
#endif
                return;
            }

            if (optionRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: オプション画面のルートVisualElementが見つかりません。{OPTION_SCREEN_NAME}");
#endif
                return;
            }

            if (creditRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: クレジット画面のルートVisualElementが見つかりません。{CREDIT_SCREEN_NAME}");
#endif
                return;
            }

            TitleSceneView titleSceneView = new(titleRoot, _outGameUIEvent, titleStartController, _currentSceneName, _targetSceneName);
            MenuScreenView menuScreenView = new(menuRoot, _outGameUIEvent);
            OptionsScreenView optionsScreenView = new(optionRoot, _outGameUIEvent);
            CreditScreenView creditScreenView = new(creditRoot, _outGameUIEvent);

            _titleScreenViewRegistry = new TitleScreenViewRegistry(titleSceneView, menuScreenView, optionsScreenView, creditScreenView);

            IScreenStateRepository screenStateRepository = new ScreenStateRepository();
            IScreenRuleRepository screenRuleRepository = new ScreenRuleRepository(_ruleData);

            IScreenTransitionApplicable screenViewModel = new ScreenViewApplicator(_titleScreenViewRegistry);
            IScreenPresenter screenPresenter = new ScreenPresenter(screenViewModel);

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

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SavedataSystemがServiceLocatorに登録されていません。");
#endif
                return;
            }

            bool isFirstLaunch = !savedataSystem.Exists<SaveData>();

            _saveData = await savedataSystem.LoadAsync<SaveData>();
            RegisterUIEventCallbacks();

            _screenController.ShowTitle();

            if (isFirstLaunch)
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: 初回起動時の遷移先シーンを設定します。{_firstLaunchTargetSceneName}");
#endif
                titleSceneView.SetTargetSceneName(_firstLaunchTargetSceneName);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: セーブデータが存在するため、通常の遷移先シーンを設定します。{_targetSceneName}");
#endif
                titleSceneView.SetTargetSceneName(_targetSceneName);
            }
        }

        /// <summary>
        ///   コールバックの登録を解除する。
        /// </summary>
        private void OnDestroy()
        {
            UnRegisterUIEventCallbacks();
        }

        /// <summary>
        ///    OutGameUIEventのコールバックを登録する。
        /// </summary>
        private void RegisterUIEventCallbacks()
        {
            _outGameUIEvent.OnShowTitleScreen += HandleTitleScreenShown;
            _outGameUIEvent.OnShowMenuScreen += HandleMenuScreenShown;
            _outGameUIEvent.OnShowOptionsScreen += HandleOptionsScreenShown;
            _outGameUIEvent.OnShowCreditScreen += HandleCreditScreenShown;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
        }

        /// <summary>
        ///   OutGameUIEventのコールバックを解除する。
        /// </summary>
        private void UnRegisterUIEventCallbacks()
        {
            _outGameUIEvent.OnShowTitleScreen -= HandleTitleScreenShown;
            _outGameUIEvent.OnShowMenuScreen -= HandleMenuScreenShown;
            _outGameUIEvent.OnShowOptionsScreen -= HandleOptionsScreenShown;
            _outGameUIEvent.OnShowCreditScreen -= HandleCreditScreenShown;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;
        }


        /// <summary>
        ///     タイトル画面を表示する処理を行う。
        /// </summary>
        private void HandleTitleScreenShown()
        {
            _screenController.ShowTitle();
        }

        /// <summary>
        ///    メニュー画面を表示する処理を行う。
        /// </summary>
        private void HandleMenuScreenShown()
        {
            _screenController.ShowMenu();
        }

        /// <summary>
        ///    オプション画面を表示する処理を行う。
        /// </summary>
        private void HandleOptionsScreenShown()
        {
            _screenController.ShowOptions();
        }

        /// <summary>
        ///     クレジット画面を表示する処理を行う。
        /// </summary>
        private void HandleCreditScreenShown()
        {
            _screenController.ShowCredit();
        }

        /// <summary>
        ///    現在表示されている画面を閉じる処理を行う。
        /// </summary>
        private void HandleScreenClosed()
        {
            _screenController.CloseCurrentImmediately();
        }
    }
}
