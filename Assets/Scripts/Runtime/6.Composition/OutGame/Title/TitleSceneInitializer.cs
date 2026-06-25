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
        private const string TITLE_OPTION_SCREEN_NAME = "TitleOptionContainer";
        private const string OPTION_SCREEN_NAME = "OptionContainer";
        private const string CREDIT_SCREEN_NAME = "CreditContainer";

        [SerializeField, Tooltip("UI Document")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("画面遷移ルールデータ")]
        private ScreenRuleData _ruleData;

        [SerializeField, SceneNameSelector, Tooltip("遷移元のシーン名")]
        private string _currentSceneName;

        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名")]
        private string _targetSceneName;

        // セーブデータの読み込み結果を保持するフィールド
        private SaveData _saveData;

        private OutGameUIEvent _outGameUIEvent;
        private TitleScreenViewRegistry _titleScreenViewRegistry;
        private ScreenController _screenController;

        private void Start()
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
            var titleOptionRoot = root.Q<VisualElement>(TITLE_OPTION_SCREEN_NAME);
            var optionRoot = root.Q<VisualElement>(OPTION_SCREEN_NAME);
            var creditRoot = root.Q<VisualElement>(CREDIT_SCREEN_NAME);
            if (titleRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: タイトル画面のルートVisualElementが見つかりません。{TITLE_SCREEN_NAME}");
#endif
                return;
            }

            if (titleOptionRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: タイトルオプション画面のルートVisualElementが見つかりません。{TITLE_OPTION_SCREEN_NAME}");
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

            TitleSceneView titleSceneView = new (titleRoot, _outGameUIEvent, titleStartController, _currentSceneName, _targetSceneName);
            TitleOptionScreenView titleOptionScreenView = new (titleOptionRoot, _outGameUIEvent);
            // TODO : オプション画面とクレジット画面の view を作成する。

            _titleScreenViewRegistry = new TitleScreenViewRegistry(titleSceneView, titleOptionScreenView);


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

            LoadSaveData();
            RegisterUIEventCallbacks();

            _screenController.ShowTitle();
        }

        /// <summary>
        ///   コールバックの登録を解除する。
        /// </summary>
        private void OnDestroy()
        {
            UnRegisterUIEventCallbacks();
        }

        /// <summary>
        ///     SaveDataを非同期で読み込む。
        /// </summary>
        private void LoadSaveData()
        {
            if (ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                ValueTask<SaveData> savedata = savedataSystem.LoadAsync<SaveData>();
                if (savedata.IsCompletedSuccessfully)
                {
                    _saveData = savedata.Result;
                    Debug.Log($"{nameof(TitleSceneInitializer)}: SaveDataの読み込みに成功しました。");
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError($"{nameof(TitleSceneInitializer)}: SaveDataの読み込みに失敗しました。");
#endif
                }
            }
        }

        /// <summary>
        ///    OutGameUIEventのコールバックを登録する。
        /// </summary>
        private void RegisterUIEventCallbacks()
        {
            _outGameUIEvent.OnShowTitleScreen += HandleTitleScreenShown;
            _outGameUIEvent.OnShowTitleOptionScreen += HandleTitleOptionScreenShown;
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
            _outGameUIEvent.OnShowTitleOptionScreen -= HandleTitleOptionScreenShown;
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
        ///    タイトルオプション画面を表示する処理を行う。
        /// </summary>
        private void HandleTitleOptionScreenShown()
        {
            _screenController.ShowTitleOption();
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
