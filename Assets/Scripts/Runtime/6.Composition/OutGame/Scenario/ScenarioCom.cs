using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.Scenario;
using KillChord.Runtime.Utility.Identity;
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
        [SerializeField, Tooltip("Autoボタンがあるシーンのみ設定する状態表示View。")]
        private ScenarioAutoButtonView _scenarioAutoButtonView;
        private ScenarioUsecase _usecase;
        private ScenarioInputController _inputController;
        private ScenarioViewModel _viewModel;
        private InputComposition _inputComposition;
        private SelectedScenarioState _selectedScenarioState;
        private OutGameUIEvent _outGameUIEvent;
        private OutGameSortieController _outGameSortieController;
        private SceneTransitionController _sceneTransitionController;
        private PendingNodeTransitionState _pendingNodeTransitionState;
        private StageProgressSaveDataService _stageProgressSaveDataService;
        private BackgroundCatalogAsset _loadedBackgroundCatalog;
        private AnimationCatalogAsset _loadedAnimationCatalog;
        private PortraitCatalogAsset _loadedPortraitCatalog;
        private ScenarioSettingsAsset _loadedScenarioSettings;
        private bool _isInitialized;
        private bool _isShuttingDown;
        private int _runGeneration;

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
            _viewModel = new ScenarioViewModel();
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
                _viewModel,
                _viewModel);

            _usecase = new ScenarioUsecase(
                repository,
                handlerRepo,
                gate,
                presenterFacade,
                presenterFacade,
                scenarioSettingsRepository);
            TextEventHandler textHandle = new TextEventHandler(
                presenterFacade,
                _usecase,
                _usecase,
                scenarioSettingsRepository);
            _inputController = new ScenarioInputController(gate, textHandle, _usecase, _usecase);
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
            if (_scenarioAutoButtonView != null) { _scenarioAutoButtonView.Initialize(_viewModel); }
            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// 他モジュールとの結合を行い、シナリオ再生を開始する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            // 初期化済みで、依存するサービスが揃っているかを確認する。
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

            // 無くても動作するサービスは、あれば取得する。
            ServiceLocator.TryGetInstance(out _outGameUIEvent);
            ServiceLocator.TryGetInstance(out _outGameSortieController);

            if (!ServiceLocator.TryGetInstance(out _sceneTransitionController))
            {
                Debug.LogError($"[{nameof(ScenarioCom)}] SceneTransitionController が取得できませんでした。", this);
                return false;
            }

            // セーブと、次のノードへの遷移の予約を用意する。
            _stageProgressSaveDataService = new StageProgressSaveDataService();

            if (!ServiceLocator.TryGetInstance(out _pendingNodeTransitionState))
            {
                _pendingNodeTransitionState = new PendingNodeTransitionState();
                ServiceLocator.RegisterInstance(_pendingNodeTransitionState);
            }

            // 入力を初期化し、シナリオの入力を有効にしてから再生を始める。
            _scenarioInputView.Initialize(_inputController, _inputComposition.GetInputView, _viewModel);
            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.Scenario);
            _isShuttingDown = false;
            int runGeneration = ++_runGeneration;
            _ = RunScenarioAsync(runGeneration);
            return true;
        }

        /// <summary>
        /// 無効化時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDisable()
        {
            StopScenarioPlayback();
        }

        /// <summary>
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            StopScenarioPlayback();
        }

        /// <summary>
        /// 登録済みの状態とロード済みアセットを解放する。
        /// </summary>
        public override void Shutdown()
        {
            StopScenarioPlayback();
            ReleaseAssets();
            _usecase = null;
            _inputController = null;
            _viewModel = null;
            _inputComposition = null;
            _selectedScenarioState = null;
            _outGameUIEvent = null;
            _outGameSortieController = null;
            _sceneTransitionController = null;
            _pendingNodeTransitionState = null;
            _stageProgressSaveDataService = null;
            _isInitialized = false;
        }

        /// <summary>
        /// シナリオ進行とシーン復帰を非同期で実行する。
        /// </summary>
        private async Task RunScenarioAsync(int runGeneration)
        {
            // 外側ではキャンセルとシーン復帰自体の例外を受け止め、非同期実行を終了する。
            bool hasRequestedDedicatedBattle = false;
            try
            {
                // 進行中の例外は内側で記録し、正常完了と同じOutGame復帰処理へ合流させる。
                try
                {
                    while (true)
                    {
                        if (!CanContinueRun(runGeneration))
                        {
                            return;
                        }

                        ScenarioStageDefinition completedStageDefinition =
                            _selectedScenarioState.CurrentStageDefinition;
                        try
                        {
                            _scenarioInputView.RestoreUIForPlayback();
                            if (!_scenarioView.gameObject.activeSelf)
                            {
                                _scenarioView.gameObject.SetActive(true);
                            }
                            _scenarioView.PrepareForPlayback();
                            await _usecase.PlayScenario(_selectedScenarioState.CurrentScenarioId);
                        }
                        finally
                        {
                            if (_scenarioView != null) { _scenarioView.EndPlayback(); }
                        }

                        if (!CanContinueRun(runGeneration))
                        {
                            return;
                        }

                        await CompleteScenarioStageAsync(completedStageDefinition);
                        if (!CanContinueRun(runGeneration))
                        {
                            return;
                        }

                        hasRequestedDedicatedBattle = _pendingNodeTransitionState != null
                            && _pendingNodeTransitionState.TryPeekCompleted(out PendingNodeTransition candidate)
                            && candidate.TargetStageDefinition is BattleStageDefinition;
                        ScenarioTransitionResult transitionResult = await TryExecutePendingNodeTransitionAsync();
                        if (transitionResult == ScenarioTransitionResult.ContinueScenario)
                        {
                            continue;
                        }

                        if (transitionResult == ScenarioTransitionResult.EndRun)
                        {
                            // 専用出撃後はScenarioが破棄されるため、View・入力・選択へ触れません。
                            return;
                        }
                        hasRequestedDedicatedBattle = false;

                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // キャンセル時は復帰処理を開始せず、外側へ渡して終了する。
                    throw;
                }
                catch (Exception exception)
                {
                    // 専用出撃の例外を再生・保存失敗の通常帰還へ流さず、外側で終端します。
                    if (hasRequestedDedicatedBattle) { throw; }
                    Debug.LogException(exception, this);
                    // 再生・保存の失敗時も、以下の共通復帰処理へ進む。
                }

                if (!CanContinueRun(runGeneration))
                {
                    return;
                }

                string scenarioSceneName = gameObject.scene.name;
                SelectedScenarioState selectedScenarioState = _selectedScenarioState;
                int selectionRevision = selectedScenarioState.SelectionRevision;

                bool transitioned = _outGameSortieController != null
                    ? await _outGameSortieController.ReturnFromScenarioAsync(
                        scenarioSceneName,
                        _returnSceneName)
                    : await ReturnToUnloadedOutGameAsync(scenarioSceneName);

                if (!transitioned)
                {
                    Debug.LogError($"[{nameof(ScenarioCom)}] シーン復帰に失敗しました。", this);
                    return;
                }

                selectedScenarioState.TryClear(selectionRevision);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// 現在の非同期再生がまだCompositionに所有されているか確認する。
        /// </summary>
        private bool CanContinueRun(int runGeneration)
        {
            return !_isShuttingDown
                && _isInitialized
                && runGeneration == _runGeneration
                && _usecase != null
                && _scenarioView != null
                && _scenarioInputView != null
                && _selectedScenarioState != null;
        }

        /// <summary>
        /// 保存を伴う通常Skipとは分けて、Composition所有の再生を停止する。
        /// </summary>
        private void StopScenarioPlayback()
        {
            if (_isShuttingDown)
            {
                return;
            }

            _isShuttingDown = true;
            _runGeneration++;
            if (_scenarioInputView != null) { _scenarioInputView.ClearSkipConfirmation(); }
            if (_usecase != null) { _usecase.RequestSkip(); }
            if (_scenarioView != null) { _scenarioView.EndPlayback(); }
        }

        /// <summary>
        ///     タイトルから直接開いたシナリオを終了し、OutGameを新たにロードします。
        /// </summary>
        /// <param name="scenarioSceneName"> 終了するシナリオシーン名です。 </param>
        /// <returns> OutGameへの復帰に成功した場合はtrueです。 </returns>
        private async Task<bool> ReturnToUnloadedOutGameAsync(string scenarioSceneName)
        {
            bool loadSuccess = await _sceneTransitionController.LoadAdditiveAsync(
                _returnSceneName,
                destroyCancellationToken);
            if (!loadSuccess)
            {
                return false;
            }

            // ScenarioのShutdownより前に、ロード抑止解除後の入力マップをOutGameへ予約する。
            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
            return await _sceneTransitionController.UnloadWithPersistentLifetimeAsync(
                scenarioSceneName);
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

            await _stageProgressSaveDataService.SaveClearAsync(
                stageDefinition.StageId,
                stageDefinition.FirstClearReward,
                stageDefinition.ClearReward,
                _selectedScenarioState.IsOpeningTutorialScenario);
            _pendingNodeTransitionState?.MarkCompleted(stageDefinition.StageId);
            _outGameUIEvent?.OnStageCleared?.Invoke(stageDefinition.StageId.Value);
        }

        /// <summary>
        ///     予約済みのノード連結を実行します。
        /// </summary>
        /// <returns> 実行結果。 </returns>
        private async Task<ScenarioTransitionResult> TryExecutePendingNodeTransitionAsync()
        {
            PendingNodeTransitionState pending = _pendingNodeTransitionState;
            OutGameSortieController sortie = _outGameSortieController;
            if (pending == null || sortie == null
                || !pending.TryPeekCompleted(out PendingNodeTransition candidate))
            {
                return ScenarioTransitionResult.None;
            }
            if (ServiceLocator.TryGetInstance(out SceneTransitionController transition)
                && transition.HasScenarioBattleSortie)
            {
                return ScenarioTransitionResult.EndRun;
            }

            if (candidate.TargetStageDefinition is ScenarioStageDefinition scenarioStageDefinition)
            {
                if (!pending.TryConsumeCompleted(out _)) { return ScenarioTransitionResult.None; }
                _selectedScenarioState.SelectScenario(scenarioStageDefinition);
                return ScenarioTransitionResult.ContinueScenario;
            }
            if (candidate.TargetStageDefinition is not BattleStageDefinition)
            {
                return ScenarioTransitionResult.None;
            }

            // アンロード前に必要な値を保持し、await後はCompositionのフィールドを使用しません。
            string sceneName = gameObject.scene.name;
            int selectionRevision = _selectedScenarioState.SelectionRevision;
            NodeTransitionExecutor executor = new(sortie);
            ScenarioBattleSortieResult result = await executor.TryExecuteAsync(candidate, sceneName, selectionRevision);
            switch (result)
            {
                case ScenarioBattleSortieResult.Started:
                case ScenarioBattleSortieResult.Failed:
                    return ScenarioTransitionResult.EndRun;

                case ScenarioBattleSortieResult.PreparationFailed:
                    return ScenarioTransitionResult.None;

                case ScenarioBattleSortieResult.Busy:
                    if (_sceneTransitionController.HasScenarioBattleSortie)
                    {
                        return ScenarioTransitionResult.EndRun;
                    }

                    // 専用処理の所有者がいないため、予約を整理して通常帰還へ進みます。
                    pending.Clear();
                    return ScenarioTransitionResult.None;

                default:
                    pending.Clear();
                    return ScenarioTransitionResult.None;
            }
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
            /// <summary> 専用出撃側が処理を所有するため、この再生処理を終了する。 </summary>
            EndRun,
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
