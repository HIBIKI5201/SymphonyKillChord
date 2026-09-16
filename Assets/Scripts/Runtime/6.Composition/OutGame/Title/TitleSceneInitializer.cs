using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.Screen;
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
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Title;
using KillChord.Runtime.View.Persistent.Input;
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

        [SerializeField, SourceDataAddress, Tooltip("敵Wave定義リポジトリの Addressables キーです。")]
        private string _enemyWaveDefinitionRepositoryKey = "EnemyWaveDefinitionRepository";

        [SerializeField, Tooltip("クレジット画面に表示する制作メンバー CSV です。列は 名前,役職,所属 の順です。")]
        private TextAsset _memberCsv;

        private OutGameUIEvent _outGameUIEvent;
        private TitleScreenViewRegistry _titleScreenViewRegistry;
        private TitleSceneView _titleSceneView;
        private TitleStartController _titleStartController;
        private ScreenController _screenController;
        private ScreenRuleData _loadedRuleData;
        private StageTreeAsset _loadedStageTreeAsset;
        private EnemyWaveDefinitionRepository _loadedEnemyWaveDefinitionRepository;
        private SaveData _loadedSaveData;
        private AudioSettingsModuleContainer _audioSettingsContainer;
        private VolumeSettingsTabView _volumeSettingsTabView;

        private bool _isInitialized;
        private bool _isSubscribed;
        private bool _isLoadingSubscribed;
        private bool _isResettingSaveData;
        private LoadingScreenController _loadingScreenController;

        /// <summary>
        ///     タイトル画面に必要なアセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(System.Threading.CancellationToken cancellationToken)
        {
            _loadedRuleData = await _ruleDataKey.LoadAssetAsync<ScreenRuleData>(this, cancellationToken);
            _loadedStageTreeAsset =
                await _stageTreeAssetKey.LoadAssetAsync<StageTreeAsset>(this, cancellationToken);
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

            var titleRoot = root.Q<VisualElement>(TITLE_SCREEN_NAME);
            var menuRoot = root.Q<VisualElement>(MENU_SCREEN_NAME);
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

            if (creditRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: クレジット画面のルート VisualElement が見つかりません。{CREDIT_SCREEN_NAME}");
#endif
                return false;
            }

            _titleSceneView = new(titleRoot, _outGameUIEvent, _titleStartController, _currentSceneName, _targetSceneName);

            // コントローラーのOptionsボタンからオプション画面を開けるようにする。
            if (ServiceLocator.TryGetInstance(out PlayerInputView playerInputView))
            {
                _titleSceneView.BindOptionInput(playerInputView);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(TitleSceneInitializer)}: PlayerInputView が ServiceLocator に登録されていません。"
                    + " Optionsボタンでのオプション表示は無効になります。");
            }
            HierarchicalNavigationScope creditNavgationScope = new(creditRoot);

            MenuScreenView menuScreenView = new(menuRoot, _outGameUIEvent);
            CreditScreenView creditScreenView = new(creditRoot, _outGameUIEvent, creditNavgationScope);
            _volumeSettingsTabView = new VolumeSettingsTabView(
                menuRoot,
                _audioSettingsContainer.ViewModel,
                _audioSettingsContainer.Command);

            _titleScreenViewRegistry = new TitleScreenViewRegistry(_titleSceneView, menuScreenView, creditScreenView);

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

            if (!ApplyStartDestination())
            {
                return false;
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

            if (!ServiceLocator.TryGetInstance(out _loadingScreenController))
            {
                Debug.LogError($"[{nameof(TitleSceneInitializer)}] LoadingScreenControllerを取得できませんでした。", this);
                return false;
            }

            if (!_isLoadingSubscribed)
            {
                _loadingScreenController.LoadingStarted += HandleLoadingStarted;
                _loadingScreenController.LoadingCompleted += HandleLoadingCompleted;
                _isLoadingSubscribed = true;
            }

            ApplyInteractionEnabled(!_loadingScreenController.IsLoading);
            RegisterUIEventCallbacks();
            _titleScreenViewRegistry.ResetFocusHistory();
            _screenController.ShowTitle();
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            UnsubscribeLoading();
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
            _audioSettingsContainer = null;
            _titleScreenViewRegistry?.Dispose();
            _titleScreenViewRegistry = null;
            _titleSceneView = null;
            _titleStartController = null;
            _screenController = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
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
                _titleScreenViewRegistry?.SetInteractionEnabled(isEnabled);
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
            _titleScreenViewRegistry.ResetFocusHistory();
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
            if (_isResettingSaveData)
            {
                return;
            }

            _isResettingSaveData = true;
            ApplyInteractionEnabled(false);
            // リセット前の音量設定を保持する。
            AudioSettingsData preservedAudioSettings = GetPreservedAudioSettings();
            bool canResumeInteraction = false;

            try
            {
                await SaveStore.DeleteAsync<SaveData>();

                // セーブデータをロードして、初期状態に戻す。
                _loadedSaveData = await LoadSaveData();
                if (_loadedSaveData == null)
                {
                    Debug.LogError(
                        $"[{nameof(TitleSceneInitializer)}] リセット後のセーブデータをロードできませんでした。",
                        this);
                    return;
                }

                await ApplyInitialSkillLoadoutAsync();
                await ApplyPreservedAudioSettingsAsync(preservedAudioSettings);

                canResumeInteraction = ApplyStartDestination();
                if (!canResumeInteraction)
                {
                    Debug.LogError(
                        $"[{nameof(TitleSceneInitializer)}] リセット後の遷移先を設定できませんでした。",
                        this);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (!canResumeInteraction)
                {
                    canResumeInteraction = await TryRecoverStartDestinationAsync();
                }

                ApplyInteractionEnabled(canResumeInteraction);
                _isResettingSaveData = false;
            }
        }

        /// <summary>
        ///     リセット失敗後にセーブデータと開始遷移先を復旧します。
        /// </summary>
        /// <returns> 開始操作を安全に再開できる場合はtrueです。 </returns>
        private async ValueTask<bool> TryRecoverStartDestinationAsync()
        {
            _loadedSaveData = await LoadSaveData();
            if (_loadedSaveData != null && ApplyStartDestination())
            {
                return true;
            }

            Debug.LogError(
                $"[{nameof(TitleSceneInitializer)}] 開始遷移先を復旧できないため、タイトル操作を停止します。",
                this);
            return false;
        }

        /// <summary>
        ///     チュートリアル進行状態に応じてタイトルからの遷移先を設定します。
        /// </summary>
        /// <returns> 遷移先を設定できた場合はtrueです。 </returns>
        private bool ApplyStartDestination()
        {
            if (_titleSceneView == null || _loadedSaveData == null)
            {
                return false;
            }

            if (_loadedSaveData.Tutorial.Phase != TutorialPhase.NotStarted)
            {
                if (ServiceLocator.TryGetInstance(out SelectedScenarioState existingScenarioState))
                {
                    existingScenarioState.Clear();
                }

                _titleSceneView.SetTargetSceneName(_targetSceneName);
                return true;
            }

            if (!TryGetOpeningScenario(out ScenarioStageDefinition openingScenario))
            {
                Debug.LogError(
                    $"[{nameof(TitleSceneInitializer)}] 起点となるチュートリアルシナリオがありません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedScenarioState selectedScenarioState))
            {
                selectedScenarioState = new SelectedScenarioState();
                if (!ServiceLocator.RegisterInstance(selectedScenarioState))
                {
                    Debug.LogError(
                        $"[{nameof(TitleSceneInitializer)}] {nameof(SelectedScenarioState)} を登録できませんでした。",
                        this);
                    return false;
                }
            }

            selectedScenarioState.SelectOpeningTutorialScenario(openingScenario);
            _titleSceneView.SetTargetSceneName(openingScenario.TargetSceneName);
            return true;
        }

        /// <summary>
        ///     選択中のゲームデータから前提ノードを持たない最初のシナリオを取得します。
        /// </summary>
        /// <param name="scenarioStageDefinition"> 取得したシナリオステージです。 </param>
        /// <returns> 対象を取得できた場合はtrueです。 </returns>
        private bool TryGetOpeningScenario(out ScenarioStageDefinition scenarioStageDefinition)
        {
            scenarioStageDefinition = null;
            if (_loadedStageTreeAsset == null || _loadedEnemyWaveDefinitionRepository == null)
            {
                return false;
            }

            StageTree stageTree = _loadedStageTreeAsset.Create(_loadedEnemyWaveDefinitionRepository);
            for (int i = 0; i < stageTree.Nodes.Count; i++)
            {
                StageNode node = stageTree.Nodes[i];
                if (node.Definition is ScenarioStageDefinition candidate
                    && stageTree.GetPreviousIds(node.Id).Count == 0)
                {
                    scenarioStageDefinition = candidate;
                    return true;
                }
            }

            return false;
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
