using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Music;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.OutGame.Screen;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Title;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.SaveSystem;
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
    public sealed class TitleSceneInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(TitleSceneInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 20;

        private const string TITLE_SCREEN_NAME = "TitleContainer";
        private const string MENU_SCREEN_NAME = "MenuContainer";
        private const string OPTION_SCREEN_NAME = "OptionContainer";
        private const string CREDIT_SCREEN_NAME = "CreditContainer";

        [SerializeField, Tooltip("UI Document")]
        private UIDocument _uiDocument;

        [SerializeField, SourceDataAddress, Tooltip("画面遷移ルールデータの Addressables キーです。")]
        private string _ruleDataKey;

        [Header("シーン遷移設定")]
        [SerializeField, SceneNameSelector, Tooltip("遷移元のシーン名")]
        private string _currentSceneName;

        [SerializeField, SceneNameSelector, Tooltip("遷移先のシーン名")]
        private string _targetSceneName;

        [SerializeField, SourceDataAddress, Tooltip("ステージツリー定義アセットの Addressables キーです。")]

        private string _stageTreeAssetKey = "StageTreeAsset";

        [SerializeField, SourceDataAddress, Tooltip("敵Wave定義リポジトリの Addressables キーです。バトルシーン名の解決に使用します。")]
        private string _enemyWaveDefinitionRepositoryKey = "EnemyWaveDefinitionRepository";

        [SerializeField, Tooltip("クレジット画面に表示する制作メンバー CSV です。列は 名前,役職,所属 の順です。")]
        private TextAsset _memberCsv;

        private OutGameUIEvent _outGameUIEvent;
        private TitleScreenViewRegistry _titleScreenViewRegistry;
        private TitleSceneView _titleSceneView;
        private TitleStartController _titleStartController;
        private ScreenController _screenController;
        private BattleSortieSelectionService _battleSortieSelectionService;
        private ScreenRuleData _loadedRuleData;
        private StageTreeAsset _loadedStageTreeAsset;
        private EnemyWaveDefinitionRepository _loadedEnemyWaveDefinitionRepository;
        private SaveData _loadedSaveData;
        private AudioSettingsModuleContainer _audioSettingsContainer;
        private VolumeSettingsTabView _volumeSettingsTabView;
        private DataResetTabView _dataResetTabView;

        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     タイトル画面に必要なアセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(System.Threading.CancellationToken cancellationToken)
        {
            _loadedRuleData = await _ruleDataKey.LoadAssetAsync<ScreenRuleData>(this, cancellationToken);
            _loadedStageTreeAsset = await _stageTreeAssetKey.LoadAssetAsync<StageTreeAsset>(this, cancellationToken);
            _loadedEnemyWaveDefinitionRepository =
                await _enemyWaveDefinitionRepositoryKey.LoadAssetAsync<EnemyWaveDefinitionRepository>(
                    this,
                    cancellationToken);
            _loadedSaveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            return _loadedRuleData != null
                && _loadedStageTreeAsset != null
                && _loadedEnemyWaveDefinitionRepository != null
                && _loadedSaveData != null;
        }

        /// <summary>
        ///     タイトル画面を構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: UI Document が設定されていません。");
#endif
                return false;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: Root VisualElement が null です。");
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

            SceneTransitionController sceneTransitionController;
            if (!TryGetServiceLocatorInstances(
                    out sceneTransitionController,
                    out _audioSettingsContainer))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: ServiceLocator から必要なインスタンスを取得できませんでした。");
#endif
                return false;
            }

            _titleStartController = new(sceneTransitionController);
            _battleSortieSelectionService = new BattleSortieSelectionService();

            var titleRoot = root.Q<VisualElement>(TITLE_SCREEN_NAME);
            var menuRoot = root.Q<VisualElement>(MENU_SCREEN_NAME);
            var optionRoot = root.Q<VisualElement>(OPTION_SCREEN_NAME);
            var creditRoot = root.Q<VisualElement>(CREDIT_SCREEN_NAME);
            if (titleRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: タイトル画面のルート VisualElement が見つかりません。{TITLE_SCREEN_NAME}");
#endif
                return false;
            }

            if (menuRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: メニュー画面のルート VisualElement が見つかりません。{MENU_SCREEN_NAME}");
#endif
                return false;
            }

            if (optionRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: オプション画面のルート VisualElement が見つかりません。{OPTION_SCREEN_NAME}");
#endif
                return false;
            }

            if (creditRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: クレジット画面のルート VisualElement が見つかりません。{CREDIT_SCREEN_NAME}");
#endif
                return false;
            }

            _titleSceneView = new(titleRoot, _outGameUIEvent, _titleStartController, _currentSceneName, _targetSceneName);
            MenuScreenView menuScreenView = new(menuRoot, _outGameUIEvent);
            OptionsScreenView optionsScreenView = new(optionRoot, _outGameUIEvent);
            CreditScreenView creditScreenView = new(creditRoot, _outGameUIEvent);
            _volumeSettingsTabView = new VolumeSettingsTabView(
                optionRoot,
                _audioSettingsContainer.ViewModel,
                _audioSettingsContainer.Command);
            _dataResetTabView = new DataResetTabView(optionRoot, _outGameUIEvent);

            _titleScreenViewRegistry = new TitleScreenViewRegistry(_titleSceneView, menuScreenView, optionsScreenView, creditScreenView);

            BuildMemberList(creditScreenView);

            IScreenStateRepository screenStateRepository = new ScreenStateRepository();
            IScreenRuleRepository screenRuleRepository = new ScreenRuleRepository(_loadedRuleData);

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

            bool isTutorialCompleted = _loadedSaveData.Tutorial.IsTutorialCompleted;

            if (!isTutorialCompleted
                && TryPrepareTutorialBattleSortie(out string tutorialTargetSceneName))
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: 初回起動時の遷移先シーンを設定します。{tutorialTargetSceneName}");
#endif
                _titleStartController.SetTutorialBattleTarget(tutorialTargetSceneName);
                _titleSceneView.SetTargetSceneName(tutorialTargetSceneName);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(TitleSceneInitializer)}: セーブデータが存在するため、通常の遷移先シーンを設定します。{_targetSceneName}");
#endif
                _titleStartController.ClearTutorialBattleTarget();
                _titleSceneView.SetTargetSceneName(_targetSceneName);
            }

            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     他モジュールとの結合と初期表示を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            RegisterUIEventCallbacks();
            _screenController.ShowTitle();
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_outGameUIEvent != null && _isSubscribed)
            {
                UnRegisterUIEventCallbacks();
            }

            _ruleDataKey.ReleaseLoadedAsset(this);
            _stageTreeAssetKey.ReleaseLoadedAsset(this);
            _enemyWaveDefinitionRepositoryKey.ReleaseLoadedAsset(this);
            _loadedRuleData = null;
            _loadedStageTreeAsset = null;
            _loadedEnemyWaveDefinitionRepository = null;
            _loadedSaveData = null;
            _volumeSettingsTabView?.Dispose();
            _volumeSettingsTabView = null;
            _dataResetTabView?.Dispose();
            _dataResetTabView = null;
            _audioSettingsContainer = null;
            _titleScreenViewRegistry = null;
            _titleSceneView = null;
            _titleStartController = null;
            _battleSortieSelectionService = null;
            _screenController = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        /// <summary>
        ///    ServiceLocator から必要なインスタンスを取得する。
        /// </summary>
        /// <param name="sceneTransitionController"></param>
        /// <param name="audioSettingsContainer"></param>
        /// <returns></returns>
        private bool TryGetServiceLocatorInstances(
            out SceneTransitionController sceneTransitionController,
            out AudioSettingsModuleContainer audioSettingsContainer)
        {
            sceneTransitionController = null;
            audioSettingsContainer = null;

            if (!ServiceLocator.TryGetInstance(out sceneTransitionController))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SceneTransitionController が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out audioSettingsContainer))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: AudioSettingsModuleContainer が ServiceLocator に登録されていません。");
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        ///     制作メンバー CSV を読み込み、クレジット画面へ一覧を反映します。
        /// </summary>
        /// <param name="creditScreenView"> 一覧の反映先となるクレジット画面 View です。 </param>
        private void BuildMemberList(CreditScreenView creditScreenView)
        {
            if (_memberCsv == null)
            {
                Debug.LogWarning(
                    $"[{nameof(TitleSceneInitializer)}] 制作メンバー CSV が設定されていないため、クレジット画面のメンバー一覧は空になります。",
                    this);
                return;
            }

            IMemberRepository memberRepository = new MemberCsvRepository(_memberCsv.text);
            IMemberListPresenter memberListPresenter = new MemberListPresenter(creditScreenView);
            ShowMemberListUseCase showMemberListUseCase = new(memberRepository, memberListPresenter);
            showMemberListUseCase.Execute();
        }

        /// <summary>
        ///    OutGameUIEventのコールバックを登録する。
        /// </summary>
        private void RegisterUIEventCallbacks()
        {
            if (_outGameUIEvent == null || _isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnShowTitleScreen += HandleTitleScreenShown;
            _outGameUIEvent.OnShowMenuScreen += HandleMenuScreenShown;
            _outGameUIEvent.OnShowOptionsScreen += HandleOptionsScreenShown;
            _outGameUIEvent.OnShowCreditScreen += HandleCreditScreenShown;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
            _outGameUIEvent.OnDataResetButtonClicked += HandleDataResetButtonClicked;
            _isSubscribed = true;
        }

        /// <summary>
        ///   OutGameUIEventのコールバックを解除する。
        /// </summary>
        private void UnRegisterUIEventCallbacks()
        {
            if (_outGameUIEvent == null || !_isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnShowTitleScreen -= HandleTitleScreenShown;
            _outGameUIEvent.OnShowMenuScreen -= HandleMenuScreenShown;
            _outGameUIEvent.OnShowOptionsScreen -= HandleOptionsScreenShown;
            _outGameUIEvent.OnShowCreditScreen -= HandleCreditScreenShown;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;
            _outGameUIEvent.OnDataResetButtonClicked -= HandleDataResetButtonClicked;
            _isSubscribed = false;
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
            _screenController.CloseCurrent();
        }

        /// <summary>
        ///     セーブデータをリセットする処理を行う。
        /// </summary>
        private async void HandleDataResetButtonClicked()
        {
            // リセット前の音量設定を保持する。
            AudioSettingsData preservedAudioSettings = GetPreservedAudioSettings();

            try
            {
                await SaveStore.DeleteAsync<SaveData>();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: セーブデータの削除中にエラーが発生しました。{ex.Message}");
#endif
                // 削除に失敗した状態で再読み込みと初期スキル補完を続けると、
                // リセットできていないデータをリセット済みとして扱ってしまう。
                return;
            }

            // セーブデータをロードして、初期状態に戻す。
            _loadedSaveData = await LoadSaveData();

            await ApplyInitialSkillLoadoutAsync();

            await ApplyPreservedAudioSettingsAsync(preservedAudioSettings);

            // セーブデータをリセットした後、初回起動時の遷移先シーンを設定します。
            if (_loadedSaveData != null
                && !_loadedSaveData.Tutorial.IsTutorialCompleted
                && TryPrepareTutorialBattleSortie(out string tutorialTargetSceneName))
            {
                _titleStartController.SetTutorialBattleTarget(tutorialTargetSceneName);
                _titleSceneView.SetTargetSceneName(tutorialTargetSceneName);
                return;
            }

            _titleStartController.ClearTutorialBattleTarget();
            _titleSceneView.SetTargetSceneName(_targetSceneName);
        }

        /// <summary>
        ///     リセット直後のセーブデータへ初期解放・初期装備スキルを補完して保存する。
        ///     <para>
        ///         補完処理は常駐シーンの起動時にしか走らないため、起動後のリセットでは
        ///         ここで明示的に呼び直す必要がある。
        ///     </para>
        /// </summary>
        private async ValueTask ApplyInitialSkillLoadoutAsync()
        {
            if (_loadedSaveData == null)
            {
                return;
            }

            if (!ServiceLocator.TryGetInstance(out InitialSkillLoadoutService initialSkillLoadoutService))
            {
                Debug.LogError(
                    $"[{nameof(TitleSceneInitializer)}] {nameof(InitialSkillLoadoutService)} が取得できませんでした。",
                    this);
                return;
            }

            if (!initialSkillLoadoutService.TryApply(_loadedSaveData))
            {
                return;
            }

            try
            {
                await SaveStore.SaveAsync<SaveData>();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[{nameof(TitleSceneInitializer)}] 初期スキルの保存中にエラーが発生しました。{ex.Message}",
                    this);
            }
        }

        /// <summary>
        ///     ViewModel が保持している現在の音量設定値を取得する。
        ///     <para>
        ///          AudioSettingsController が保持する最新値は _loadedSaveData.AudioSettings に
        ///         反映されないため、必ずViewModel経由で取得する必要がある。
        ///     </para>
        /// </summary>
        /// <returns> 現在の音量設定を表す <see cref="AudioSettingsData"/>。 </returns>
        private AudioSettingsData GetPreservedAudioSettings()
        {
            if (_audioSettingsContainer?.ViewModel == null)
            {
                return new AudioSettingsData();
            }

            IAudioSettingsViewModel viewModel = _audioSettingsContainer.ViewModel;
            return new AudioSettingsData(
                viewModel.BgmVolume.CurrentValue,
                viewModel.SoundEffectVolume.CurrentValue,
                viewModel.VoiceVolume.CurrentValue);
        }

        /// <summary>
        ///     リセット前に保持した音量設定を、リセット後のセーブデータへ反映して保存する。
        /// </summary>
        private async ValueTask ApplyPreservedAudioSettingsAsync(AudioSettingsData preservedAudioSettings)
        {
            if (_loadedSaveData == null || preservedAudioSettings == null)
            {
                return;
            }

            _loadedSaveData.AudioSettings.SetVolumes(
                preservedAudioSettings.BgmVolume,
                preservedAudioSettings.SoundEffectVolume,
                preservedAudioSettings.VoiceVolume);

            try
            {
                await SaveStore.SaveAsync<SaveData>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(TitleSceneInitializer)}: 音量設定の再保存中にエラーが発生しました。{ex.Message}");
            }
        }

        /// <summary>
        ///     初回チュートリアル用の戦闘出撃準備を行います。
        /// </summary>
        /// <param name="tutorialTargetSceneName"> 遷移先シーン名です。 </param>
        /// <returns> 準備に成功した場合はtrueです。 </returns>
        private bool TryPrepareTutorialBattleSortie(out string tutorialTargetSceneName)
        {
            tutorialTargetSceneName = string.Empty;

            if (_loadedStageTreeAsset == null
                || _loadedEnemyWaveDefinitionRepository == null
                || _battleSortieSelectionService == null)
            {
                return false;
            }

            StageTree stageTree = _loadedStageTreeAsset.Create(_loadedEnemyWaveDefinitionRepository);
            if (!stageTree.TryGetTutorialNode(out StageNode tutorialNode)
                || tutorialNode?.Definition == null)
            {
                return false;
            }

            if (tutorialNode.Definition is not BattleStageDefinition tutorialStageDefinition)
            {
                return false;
            }

            if (!_battleSortieSelectionService.TryPrepareBattleSortie(tutorialStageDefinition, _targetSceneName))
            {
                return false;
            }

            tutorialTargetSceneName = tutorialStageDefinition.TargetSceneName;
            return true;
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
                saveData = SaveStore.IsLoaded<SaveData>()
                    ? SaveStore.Get<SaveData>()
                    : await SaveStore.LoadAsync<SaveData>();
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
