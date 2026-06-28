using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Title;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading.Tasks;
using UnityEngine;
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

        private OutGameUIEvent _outGameUIEvent;
        private TitleScreenViewRegistry _titleScreenViewRegistry;
        private TitleSceneView _titleSceneView;
        private ScreenController _screenController;
        private SavedataSystem _savedataSystem;

        private async void Start()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: UI Document が設定されていません。");
#endif
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: Root VisualElement が null です。");
#endif
                return;
            }

            SceneTransitionController sceneTransitionController;
            MusicPlayer musicPlayer;
            SoundEffectVolumeManager sePlayer;
            if (!TryGetServiceLocatorInstances(out sceneTransitionController, out musicPlayer, out sePlayer, out _savedataSystem))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: ServiceLocator から必要なインスタンスを取得できませんでした。");
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
                Debug.LogError($"{nameof(TitleSceneInitializer)}: タイトル画面のルート VisualElement が見つかりません。{TITLE_SCREEN_NAME}");
#endif
                return;
            }

            if (menuRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: メニュー画面のルート VisualElement が見つかりません。{MENU_SCREEN_NAME}");
#endif
                return;
            }

            if (optionRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: オプション画面のルート VisualElement が見つかりません。{OPTION_SCREEN_NAME}");
#endif
                return;
            }

            if (creditRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: クレジット画面のルート VisualElement が見つかりません。{CREDIT_SCREEN_NAME}");
#endif
                return;
            }

            _titleSceneView = new(titleRoot, _outGameUIEvent, titleStartController, _currentSceneName, _targetSceneName);
            MenuScreenView menuScreenView = new(menuRoot, _outGameUIEvent);
            OptionsScreenView optionsScreenView = new(optionRoot, _outGameUIEvent);
            CreditScreenView creditScreenView = new(creditRoot, _outGameUIEvent);
            VolumeSettingsTabView audioVolumeTab = new(optionRoot, musicPlayer, sePlayer);
            DataResetTabView dataResetTab = new(optionRoot, _outGameUIEvent);

            _titleScreenViewRegistry = new TitleScreenViewRegistry(_titleSceneView, menuScreenView, optionsScreenView, creditScreenView);

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

            bool isFirstLaunch = !_savedataSystem.Exists<SaveData>();
            RegisterUIEventCallbacks();

            _screenController.ShowTitle();

            if (isFirstLaunch)
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: 初回起動時の遷移先シーンを設定します。{_firstLaunchTargetSceneName}");
#endif
                _titleSceneView.SetTargetSceneName(_firstLaunchTargetSceneName);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: セーブデータが存在するため、通常の遷移先シーンを設定します。{_targetSceneName}");
#endif
                _titleSceneView.SetTargetSceneName(_targetSceneName);
            }
        }

        /// <summary>
        ///   コールバックの登録を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (_outGameUIEvent != null)
            {
                UnRegisterUIEventCallbacks();
            }
        }

        /// <summary>
        ///    ServiceLocator から必要なインスタンスを取得する。
        /// </summary>
        /// <param name="sceneTransitionController"></param>
        /// <param name="musicPlayer"></param>
        /// <param name="sePlayer"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        private bool TryGetServiceLocatorInstances(
            out SceneTransitionController sceneTransitionController, out MusicPlayer musicPlayer,
            out SoundEffectVolumeManager sePlayer, out SavedataSystem savedataSystem)
        {
            sceneTransitionController = null;
            musicPlayer = null;
            sePlayer = null;
            savedataSystem = null;

            if (!ServiceLocator.TryGetInstance(out sceneTransitionController))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SceneTransitionController が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: OutGameUIEvent が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance<MusicPlayer>(out musicPlayer))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: MusicPlayer が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance<SoundEffectVolumeManager>(out sePlayer))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SoundEffectVolumeManager が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out savedataSystem))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SavedataSystem が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            return true;
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
            _outGameUIEvent.OnDataResetButtonClicked += HandleDataResetButtonClicked;
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
            _outGameUIEvent.OnDataResetButtonClicked -= HandleDataResetButtonClicked;
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

        /// <summary>
        ///     セーブデータをリセットする処理を行う。
        /// </summary>
        private async void HandleDataResetButtonClicked()
        {
            _savedataSystem.DeleteSaveData<SaveData>();

            // セーブデータをロードして、初期状態に戻す
            var newSaveData = await LoadSaveData();

            // セーブデータをリセットした後、初回起動時の遷移先シーンを設定する
            _titleSceneView.SetTargetSceneName(_firstLaunchTargetSceneName);
        }

        /// <summary>
        ///     セーブデータをロードする処理を行う。
        /// </summary>
        /// <returns></returns>
        private async ValueTask<SaveData> LoadSaveData()
        {
            SaveData saveData = null;
            try
            {
                saveData = await _savedataSystem.LoadAsync<SaveData>();
                return saveData;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: セーブデータのロード中にエラーが発生しました。{ex.Message}");
#endif
                return null;
            }
        }
    }
}
