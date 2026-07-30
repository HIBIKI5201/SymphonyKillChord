using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.Scenario;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.OutGame.Scenario;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using AnimationEventData = KillChord.Runtime.Domain.OutGame.Scenario.AnimationEvent;

namespace KillChord.Runtime.Composition.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生に必要な依存関係を組み立てて起動する。
    /// </summary>
    public sealed class ScenarioCom : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(ScenarioCom);

        /// <summary> 実行順です。 </summary>
        public override int Order => 10;

        [SerializeField]
        private ScenarioView _chatText;
        [SerializeField]
        private ScenarioInputView _inputView;
        [SerializeField, SourceDataAddress, Tooltip("背景カタログの Addressables キーです。")]
        private string _backgroundCatalogKey;
        [SerializeField, SourceDataAddress, Tooltip("アニメーションカタログの Addressables キーです。")]
        private string _animationCatalogKey;
        [SerializeField, SourceDataAddress, Tooltip("立ち絵カタログの Addressables キーです。")]
        private string _portraitCatalogKey;
        [SerializeField, SourceDataAddress, Tooltip("シナリオ設定の Addressables キーです。")]
        private string _scenarioSettingsKey;
        [SerializeField, SceneNameSelector, Tooltip("シナリオ終了後に戻るシーン名。")]
        private string _returnSceneName;
        [SerializeField, Tooltip("シナリオ表示View。Scenarioシーンに事前配置したものを指定します。")]
        private ScenarioView _scenarioView;
        [SerializeField, Tooltip("シナリオ入力View。Scenarioシーンに事前配置したものを指定します。")]
        private ScenarioInputView _scenarioInputView;
        private ScenarioUsecase _usecase;
        private ScenarioInputController _inputController;
        private ViewModel _viewModel;
        private InputComposition _inputComposition;
        private SelectedScenarioState _selectedScenarioState;
        private SceneTransitionController _sceneTransitionController;
        private OutGameUIEvent _outGameUIEvent;
        private OutGameSortieController _outGameSortieController;
        private PendingNodeTransitionState _pendingNodeTransitionState;
        private StageProgressSaveDataService _stageProgressSaveDataService;
        private BackgroundCatalogAsset _loadedBackgroundCatalog;
        private AnimationCatalogAsset _loadedAnimationCatalog;
        private PortraitCatalogAsset _loadedPortraitCatalog;
        private ScenarioSettingsAsset _loadedScenarioSettings;
        private bool _isInitialized;

        /// <summary>
        /// シナリオ用アセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> すべてロードできた場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            try
            {
            _loadedBackgroundCatalog = await _backgroundCatalogKey.LoadAssetAsync<BackgroundCatalogAsset>(this, destroyCancellationToken);
            _loadedAnimationCatalog = await _animationCatalogKey.LoadAssetAsync<AnimationCatalogAsset>(this, destroyCancellationToken);
            _loadedPortraitCatalog = await _portraitCatalogKey.LoadAssetAsync<PortraitCatalogAsset>(this, destroyCancellationToken);
            _loadedScenarioSettings = await _scenarioSettingsKey.LoadAssetAsync<ScenarioSettingsAsset>(this, destroyCancellationToken);
            }
            catch (Exception ex) { Debug.LogException(ex, this); }
            return _loadedBackgroundCatalog != null
                && _loadedAnimationCatalog != null
                && _loadedPortraitCatalog != null
                && _loadedScenarioSettings != null;
        }

        /// <summary>
        /// 依存関係を組み立ててシナリオ再生の基盤を構築する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            ScenarioAdvanceGate gate = new ScenarioAdvanceGate();
            _viewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            IScenarioRepository repository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_loadedBackgroundCatalog);
            IAnimationRepository animationRepository = new AnimationRepository(_loadedAnimationCatalog);
            IPortraitRepository portraitRepository = new PortraitRepository(_loadedPortraitCatalog);
            IScenarioSettingsRepository scenarioSettingsRepository = new ScenarioSettingsRepository(_loadedScenarioSettings);

            TextPresenter textPresenter = new TextPresenter(_viewModel);
            FadePresenter fadePresenter = new FadePresenter(_viewModel);
            BackgroundPresenter backgroundPresenter = new BackgroundPresenter(_viewModel);
            AnimationPresenter animationPresenter = new AnimationPresenter(_viewModel);
            PortraitPresenter portraitPresenter = new PortraitPresenter(_viewModel);
            LayerPresenter layerPresenter = new LayerPresenter(_viewModel);
            ScenarioPresenterFacade presenterFacade = new ScenarioPresenterFacade(
                textPresenter,
                fadePresenter,
                backgroundPresenter,
                animationPresenter,
                portraitPresenter,
                layerPresenter,
                _viewModel);

            _usecase = new ScenarioUsecase(
                repository,
                handlerRepo,
                gate,
                presenterFacade,
                scenarioSettingsRepository);
            _inputController = new ScenarioInputController(gate, _usecase, _usecase);
            TextEventHandler textHandle = new TextEventHandler(
                presenterFacade,
                _usecase,
                _usecase,
                scenarioSettingsRepository);
            FadeEventHandler fadeEventHandle = new FadeEventHandler(presenterFacade);
            BackgroundEventHandler backgroundEventHandle = new BackgroundEventHandler(presenterFacade, backgroundRepository);
            AnimationEventHandler animationEventHandle = new AnimationEventHandler(presenterFacade, animationRepository);
            PortraitEventHandler portraitEventHandler = new PortraitEventHandler(presenterFacade, portraitRepository);
            LayerEventHandler layerEventHandler = new LayerEventHandler(presenterFacade);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandle.HandleAsync);
            handlerRepo.Register<AnimationEventData>(animationEventHandle.HandleAsync);
            handlerRepo.Register<PortraitEvent>(portraitEventHandler.HandleAsync);
            handlerRepo.Register<LayerEvent>(layerEventHandler.HandleAsync);

            // View を生成する。
            var backgroundMap = BuildBackgroundMap(_loadedBackgroundCatalog);
            var animationMap = BuildAnimationMap(_loadedAnimationCatalog);
            var portraitMap = BuildPortraitMap(_loadedPortraitCatalog);

            if (_scenarioView == null || _scenarioInputView == null)
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] ScenarioView / ScenarioInputView が未設定です。", this);
                return false;
            }

            // レイヤー順は View が Domain を参照しないよう、文字列名へ変換して渡す。
            var layerOrder = new List<string>(_loadedScenarioSettings.LayerBackToFront.Count);
            foreach (ScenarioLayer layer in _loadedScenarioSettings.LayerBackToFront)
            {
                layerOrder.Add(layer.ToString());
            }

            _scenarioView.Initialize(
                _viewModel,
                backgroundMap,
                animationMap,
                portraitMap,
                layerOrder);
            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// 他モジュールとの結合を行い、シナリオ再生を開始する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _inputComposition))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] InputComposition が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _selectedScenarioState))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] SelectedScenarioState が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _sceneTransitionController))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] SceneTransitionController が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] OutGameUIEvent が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameSortieController))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] OutGameSortieController が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] SavedataSystem が取得できませんでした。", this);
                return false;
            }

            _stageProgressSaveDataService = new StageProgressSaveDataService(savedataSystem);

            if (!ServiceLocator.TryGetInstance(out _pendingNodeTransitionState))
            {
                _pendingNodeTransitionState = new PendingNodeTransitionState();
                ServiceLocator.RegisterInstance(_pendingNodeTransitionState);
            }

            _scenarioInputView.Initialize(_inputController, _inputComposition.GetInputView);
            _ = RunScenarioAsync();
            return true;
        }

        /// <summary>
        /// 無効化時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDisable()
        {
            _usecase?.RequestSkip();
        }

        /// <summary>
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            _usecase?.RequestSkip();
        }

        /// <summary>
        /// 登録済みの状態とロード済みアセットを解放する。
        /// </summary>
        public override void Shutdown()
        {
            ReleaseAssets();
            _usecase = null;
            _inputController = null;
            _viewModel = null;
            _inputComposition = null;
            _selectedScenarioState = null;
            _sceneTransitionController = null;
            _outGameUIEvent = null;
            _outGameSortieController = null;
            _pendingNodeTransitionState = null;
            _stageProgressSaveDataService = null;
            _isInitialized = false;
        }

        /// <summary>
        /// シナリオ進行とシーン復帰を非同期で実行する。
        /// </summary>
        private async Task RunScenarioAsync()
        {
            try
            {
                while (true)
                {
                    ScenarioStageDefinition completedStageDefinition =
                        _selectedScenarioState.CurrentStageDefinition;
                    await _usecase.PlayScenario(_selectedScenarioState.CurrentScenarioId);
                    await CompleteScenarioStageAsync(completedStageDefinition);

                    ScenarioTransitionResult transitionResult = TryExecutePendingNodeTransition();
                    if (transitionResult == ScenarioTransitionResult.ContinueScenario)
                    {
                        continue;
                    }

                    if (transitionResult == ScenarioTransitionResult.ExternalStageStarted)
                    {
                        _selectedScenarioState.Clear();
                        _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
                        return;
                    }

                    break;
                }

                bool transitioned = await _sceneTransitionController.UnloadAndSetActiveAsync(
                    SceneManager.GetActiveScene().name,
                    _returnSceneName,
                    destroyCancellationToken);

                if (!transitioned)
                {
                    Debug.LogError($"[{nameof(ScenarioCom)}] シーン復帰に失敗しました。", this);
                    return;
                }

                _selectedScenarioState.Clear();
                _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
                _outGameUIEvent.OnOutGameUiVisibilityChanged?.Invoke(true);
                _outGameUIEvent.OnShownHomeScreen?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     シナリオステージのクリアを保存してランタイム進行へ通知する。
        /// </summary>
        /// <param name="stageDefinition"> 完了したシナリオステージ定義。</param>
        private async Task CompleteScenarioStageAsync(ScenarioStageDefinition stageDefinition)
        {
            if (stageDefinition == null || _stageProgressSaveDataService == null)
            {
                return;
            }

            await _stageProgressSaveDataService.SaveClearAsync(stageDefinition.StageId);
            _pendingNodeTransitionState?.MarkCompleted(stageDefinition.StageId);
            _outGameUIEvent?.OnStageCleared?.Invoke(stageDefinition.StageId.Value);
        }

        /// <summary>
        ///     予約済みのノード連結を実行します。
        /// </summary>
        /// <returns> 実行結果。 </returns>
        private ScenarioTransitionResult TryExecutePendingNodeTransition()
        {
            if (_pendingNodeTransitionState == null
                || _outGameSortieController == null
                || !_pendingNodeTransitionState.TryConsumeCompleted(
                    out PendingNodeTransition pendingNodeTransition))
            {
                return ScenarioTransitionResult.None;
            }

            if (pendingNodeTransition.TargetStageDefinition
                is ScenarioStageDefinition scenarioStageDefinition)
            {
                _selectedScenarioState.SelectScenario(scenarioStageDefinition);
                return ScenarioTransitionResult.ContinueScenario;
            }

            NodeTransitionExecutor executor = new(
                new BattleSortieSelectionService(),
                _outGameSortieController);
            return executor.TryExecute(pendingNodeTransition)
                ? ScenarioTransitionResult.ExternalStageStarted
                : ScenarioTransitionResult.None;
        }

        /// <summary>
        /// 背景アセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, Sprite> BuildBackgroundMap(BackgroundCatalogAsset catalog)
        {
            var map = new Dictionary<string, Sprite>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        ///     シナリオ完了後の自動遷移実行結果。
        /// </summary>
        private enum ScenarioTransitionResult
        {
            /// <summary> 自動遷移を実行しない。 </summary>
            None,
            /// <summary> 同じシナリオシーンで次のシナリオを再生する。 </summary>
            ContinueScenario,
            /// <summary> バトルなど別ステージの開始要求に成功した。 </summary>
            ExternalStageStarted,
        }

        /// <summary>
        /// アニメーションアセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, AnimationClip> BuildAnimationMap(AnimationCatalogAsset catalog)
        {
            var map = new Dictionary<string, AnimationClip>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        /// 立ち絵アセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, Sprite> BuildPortraitMap(PortraitCatalogAsset catalog)
        {
            var map = new Dictionary<string, Sprite>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (entry.Id.Id == 0 || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        private void ReleaseAssets()
        {
            _backgroundCatalogKey.ReleaseLoadedAsset(this);
            _animationCatalogKey.ReleaseLoadedAsset(this);
            _portraitCatalogKey.ReleaseLoadedAsset(this);
            _scenarioSettingsKey.ReleaseLoadedAsset(this);
            _loadedBackgroundCatalog = null;
            _loadedAnimationCatalog = null;
            _loadedPortraitCatalog = null;
            _loadedScenarioSettings = null;
        }
    }
}
