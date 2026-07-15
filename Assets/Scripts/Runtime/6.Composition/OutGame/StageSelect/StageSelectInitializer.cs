using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Application.OutGame.StageSelect;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.StageSelect;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ選択画面の依存を解決するクラス。
    ///     UIBuilder で配置されたノード要素・接続線要素を収集して StageTree と紐付けます。
    /// </summary>
    public sealed class StageSelectInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(StageSelectInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 110;

        /// <summary> ノード要素のUSSクラス名。 </summary>
        private const string NODE_USS_CLASS = "stage-node";
        /// <summary> 接続線要素のUSSクラス名。 </summary>
        private const string CONNECTION_USS_CLASS = "stage-connection";
        /// <summary> 接続線要素のname形式。 </summary>
        private const string CONNECTION_NAME_FORMAT = "{fromId}-{toId}";
        /// <summary> ステージIDのSourceDataProviderカテゴリ。 </summary>
        private const string STAGE_ID_CATEGORY = "Stage";
        /// <summary> ステージ詳細画面のルート要素名。 </summary>
        private const string DETAIL_SCREEN_NAME = "StageDetailContainer";

        [SerializeField, Tooltip("ステージ選択画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField, RepositoryAddressSelector, Tooltip("ステージツリー定義アセットの Addressables キーです。")]
        private string _stageTreeAssetKey;

        private OutGameUIEvent _outGameUIEvent;
        private StageTree _stageTree;
        private StageProgressService _progressService;
        private StageSelectController _stageSelectController;
        private StageDetailScreenView _detailScreenView;
        private List<StageNodeView> _nodeViews;
        private List<StageNodePresenter> _nodePresenters;
        private Dictionary<StageId, StageNodePresenter> _nodePresenterMap;
        private CancellationTokenSource _cts;
        private bool _isInitialized;
        private OutGameSortieController _outGameSortieController;
        private OutGameMissionSelectController _missionSelectController;
        private string _currentSceneName;
        private StageSelectOpenUseCase _openUseCase;
        private SelectedScenarioState _selectedScenarioState;
        private SelectedBattleStageState _selectedBattleStageState;
        private SelectedMissionState _selectedMissionState;
        private StageTreeAsset _loadedStageTreeAsset;
        private SaveData _loadedSaveData;
        private bool _isSubscribed;

        /// <summary>
        ///     単体で実行できる初期化を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Init()
        {
            _currentSceneName = gameObject.scene.name;
            return true;
        }

        /// <summary>
        ///     ステージツリー定義をロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _loadedStageTreeAsset =
                await _stageTreeAssetKey.LoadAssetAsync<StageTreeAsset>(this, cancellationToken);
            if (_loadedStageTreeAsset == null
                || !ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                return false;
            }

            _loadedSaveData = await savedataSystem.LoadAsync<SaveData>();
            return _loadedSaveData != null;
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
            TryStartTutorialSortie();
            return _isInitialized;
        }

        /// <summary>
        ///     ステージノードが選択されたときのイベントハンドラ。
        /// </summary>
        private void HandleStageNodeSelected(int stageIdValue)
        {
            _stageSelectController.OnStageNodeSelected(stageIdValue, _cts.Token);
        }

        /// <summary>
        ///     ステージ詳細画面を閉じるイベントハンドラ。
        /// </summary>
        private void HandleStageDetailClosed()
        {
            _ = _detailScreenView.Hide(_cts.Token);
        }

        /// <summary>
        ///     ステージ選択画面が閉じられたときのイベントハンドラ。
        /// </summary>
        private void HandleScreenClosed()
        {
            _detailScreenView.HideImmediately();
        }

        /// <summary>
        ///     ステージクリアを受け取り、後続ノードの解放と接続線アニメーションの完了を待機するイベントハンドラ。
        /// </summary>
        private async void HandleStageCleared(int stageIdValue)
        {
            await CompleteAndAnimateAsync(new StageId(stageIdValue));
        }

        /// <summary>
        ///     出撃ボタンが押されたときの処理。
        ///     ステージタイプに応じて戦闘準備画面表示またはシナリオ直接遷移イベントを発火します。
        /// </summary>
        private async void HandleSortieRequested()
        {
            if (!_stageSelectController.TryGetSortieInfo(out StageDefinition stageDefinition))
            {
                return;
            }

            if (stageDefinition.StageType == StageType.Battle)
            {
                if (!TryPrepareBattleSortie(stageDefinition))
                {
                    return;
                }
            }
            else if (stageDefinition.StageType == StageType.Scenario)
            {
                _selectedBattleStageState.Clear();
                _selectedMissionState.Clear();

                if (string.IsNullOrWhiteSpace(stageDefinition.ScenarioId))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[{nameof(StageSelectInitializer)}] シナリオIDが設定されていません。", this);
#endif
                    return;
                }

                _selectedScenarioState.SelectScenario(stageDefinition.ScenarioId);
            }

            await _outGameSortieController.RequestSortieAsync(
                stageDefinition.StageType,
                _currentSceneName,
                stageDefinition.TargetSceneName,
                _cts.Token);
        }

        /// <summary>
        ///     初回チュートリアル要求がある場合に通常の選択状態を構築して自動出撃します。
        /// </summary>
        private void TryStartTutorialSortie()
        {
            if (!_isInitialized
                || !ServiceLocator.TryGetInstance(out TutorialSortieRequestState requestState)
                || !requestState.IsRequested)
            {
                return;
            }

            if (!_stageTree.TryGetTutorialNode(out StageNode tutorialNode)
                || !TryPrepareBattleSortie(tutorialNode.Definition))
            {
                Debug.LogError(
                    $"[{nameof(StageSelectInitializer)}] チュートリアルステージの出撃準備に失敗しました。",
                    this);
                return;
            }

            if (!_outGameSortieController.RequestImmediateBattleSortie(
                    tutorialNode.Definition.TargetSceneName)
                || !requestState.TryConsume())
            {
                Debug.LogError(
                    $"[{nameof(StageSelectInitializer)}] チュートリアル自動出撃に失敗しました。",
                    this);
                return;
            }

            ServiceLocator.UnregisterInstance<TutorialSortieRequestState>();
        }

        /// <summary>
        ///     バトルステージとミッションの選択状態を構築します。
        /// </summary>
        /// <param name="stageDefinition"> 出撃するステージ定義です。 </param>
        /// <returns> 出撃準備に成功した場合はtrueです。 </returns>
        private bool TryPrepareBattleSortie(StageDefinition stageDefinition)
        {
            if (stageDefinition == null
                || stageDefinition.StageType != StageType.Battle
                || stageDefinition.MissionDefinition == null
                || string.IsNullOrWhiteSpace(stageDefinition.BattleSceneName))
            {
                return false;
            }

            _selectedBattleStageState.SelectBattleStage(
                stageDefinition,
                _currentSceneName);
            _missionSelectController.Select(stageDefinition.MissionDefinition);
            return true;
        }

        /// <summary>
        ///     作戦画面が表示された時の処理。
        ///     セーブデータ等から新たにクリアされたステージを検出し、後続ノードの解放アニメーションを再生します。
        /// </summary>
        private async void HandleStageSelectScreenCompleted()
        {
            await ApplyNewlyClearedStagesAsync(_cts.Token);
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        private bool Initialize()
        {
            if (!ServiceLocator.TryGetInstance(out _selectedBattleStageState))
            {
                _selectedBattleStageState = new SelectedBattleStageState();

                ServiceLocator.RegisterInstance(_selectedBattleStageState);
            }

            if (!ServiceLocator.TryGetInstance(out _selectedMissionState))
            {
                _selectedMissionState = new SelectedMissionState();

                ServiceLocator.RegisterInstance(_selectedMissionState);
            }
            _missionSelectController = new OutGameMissionSelectController(_selectedMissionState);

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameSortieController))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] OutGameSortieController が取得できませんでした。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _selectedScenarioState))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] SelectedScenarioState が取得できませんでした。", this);
#endif
                return false;
            }

            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] UIDocument が設定されていません。", this);
#endif
                return false;
            }

            if (_loadedStageTreeAsset == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] StageTreeAsset が設定されていません。", this);
#endif
                return false;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            // 詳細画面のルート要素を取得する
            VisualElement detailRoot = root.Q<VisualElement>(DETAIL_SCREEN_NAME);
            if (detailRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] {DETAIL_SCREEN_NAME} が見つかりませんでした。", this);
#endif
                return false;
            }

            // --- Domain 層 ---
            _stageTree = _loadedStageTreeAsset.Create();

            // --- Application 層 ---
            _progressService = new StageProgressService(_stageTree);

            // セーブデータ連携
            IStageClearRepository clearRepository =
                new SaveDataClearStageRepository(_loadedSaveData.StageProgress);
            _openUseCase = new StageSelectOpenUseCase(_stageTree, _progressService, clearRepository);

            // Presenter 生成前に既知のクリアステージをツリーへ反映する。
            _openUseCase.ApplySavedClearedStages();

            // --- View 層（詳細画面） ---
            _detailScreenView = new StageDetailScreenView(detailRoot, _outGameUIEvent);
            _detailScreenView.HideImmediately();

            // --- View 層（接続線・ノード）---
            var connectionViewMap = BuildConnectionViewMap(root);
            BuildNodeComponents(root, connectionViewMap);

            // --- Adaptor 層 ---
            BuildControllers();

            _cts = new CancellationTokenSource();
            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
            _isSubscribed = false;
            _cts?.Cancel();
            DisposeNodeComponents();
            _cts?.Dispose();
            _cts = null;
            _stageTreeAssetKey.ReleaseLoadedAsset(this);
            _loadedStageTreeAsset = null;
            _loadedSaveData = null;
            _isInitialized = false;
        }

        /// <summary>
        ///     UIイベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || _isSubscribed) { return; }
            _outGameUIEvent.OnStageNodeSelected += HandleStageNodeSelected;
            _outGameUIEvent.OnStageDetailClosed += HandleStageDetailClosed;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
            _outGameUIEvent.OnStageCleared += HandleStageCleared;
            _outGameUIEvent.OnSortieRequested += HandleSortieRequested;
            _outGameUIEvent.OnStageSelectScreenCompleted += HandleStageSelectScreenCompleted;
            _isSubscribed = true;
        }

        /// <summary>
        ///     UIイベントの購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || !_isSubscribed) { return; }
            _outGameUIEvent.OnStageNodeSelected -= HandleStageNodeSelected;
            _outGameUIEvent.OnStageDetailClosed -= HandleStageDetailClosed;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;
            _outGameUIEvent.OnStageCleared -= HandleStageCleared;
            _outGameUIEvent.OnSortieRequested -= HandleSortieRequested;
            _outGameUIEvent.OnStageSelectScreenCompleted -= HandleStageSelectScreenCompleted;
            _isSubscribed = false;
        }

        /// <summary>
        ///     接続線 VisualElement を収集し、ToStageId をキーとした Map を構築します。
        /// </summary>
        /// <param name="root"> 検索対象のルート VisualElement。</param>
        /// <returns> ToStageId → StageNodeConnectionView の辞書。</returns>
        private Dictionary<StageId, StageNodeConnectionView> BuildConnectionViewMap(VisualElement root)
        {
            // 接続線要素の name は "{fromId}-{toId}" 形式で設定しておくこと
            // 例）stage_tutorial-stage_02
            var connectionElements = root.Query<VisualElement>(className: CONNECTION_USS_CLASS).ToList();
            var connectionViewMap = new Dictionary<StageId, StageNodeConnectionView>(connectionElements.Count);

            for (var i = 0; i < connectionElements.Count; i++)
            {
                var element = connectionElements[i];
                var parts = element.name?.Split('-');

                if (parts == null || parts.Length != 2)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] 接続線要素 '{element.name}' の name が '{CONNECTION_NAME_FORMAT}' 形式ではありません。", this);
#endif
                    continue;
                }

                int toStageIdValue = DataIDHasher.Compute(STAGE_ID_CATEGORY, parts[1]);
                if (toStageIdValue == 0)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] 接続線要素 '{element.name}' の ToStageId が未設定です。", this);
#endif
                    continue;
                }

                var toStageId = new StageId(toStageIdValue);
                connectionViewMap.Add(toStageId, new StageNodeConnectionView(element));
            }

            return connectionViewMap;
        }

        /// <summary>
        ///     ノード VisualElement を収集し、StageNodeView / StageNodePresenter を生成して各リストに登録します。
        /// </summary>
        /// <param name="root"> 検索対象のルート VisualElement。</param>
        /// <param name="connectionViewMap"> 接続線 View の辞書。</param>
        private void BuildNodeComponents(VisualElement root, Dictionary<StageId, StageNodeConnectionView> connectionViewMap)
        {
            var nodeElements = root.Query<VisualElement>(className: NODE_USS_CLASS).ToList();
            _nodeViews = new List<StageNodeView>(nodeElements.Count);
            _nodePresenters = new List<StageNodePresenter>(nodeElements.Count);
            _nodePresenterMap = new Dictionary<StageId, StageNodePresenter>(nodeElements.Count);

            // ノードのアニメーションを順番に再生するためのシーケンサーを生成する
            var sequencer = new StageNodeAnimationSequencer();

            for (var i = 0; i < nodeElements.Count; i++)
            {
                var nodeElement = nodeElements[i];
                var stageIdValue = nodeElement.name;

                // ステージノードの VisualElement の name には可読IDを設定する。
                if (string.IsNullOrEmpty(stageIdValue))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] USS クラス '{NODE_USS_CLASS}' の要素 (index:{i}) に name が設定されていません。", this);
#endif
                    continue;
                }

                int stageIdHash = DataIDHasher.Compute(STAGE_ID_CATEGORY, stageIdValue);
                if (stageIdHash == 0)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] ノード要素 '{stageIdValue}' の可読IDが未設定です。", this);
#endif
                    continue;
                }

                var stageId = new StageId(stageIdHash);
                if (!_stageTree.TryGetNode(stageId, out var node))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] StageId '{stageIdValue}' に対応するノードが StageTree に存在しません。", this);
#endif
                    continue;
                }

                var nodeView = new StageNodeView(nodeElement, stageIdHash, _outGameUIEvent);

                // このノードへの接続線Viewを取得する（存在しない場合は null）
                connectionViewMap.TryGetValue(stageId, out var incomingConnectionView);

                var nodePresenter = new StageNodePresenter(node, nodeView, incomingConnectionView, sequencer);

                _nodeViews.Add(nodeView);
                _nodePresenters.Add(nodePresenter);
                // ID で引けるようにマップへも登録する
                _nodePresenterMap.Add(stageId, nodePresenter);
            }
        }

        /// <summary>
        ///     Adaptor 層のコントローラーを構築します。
        /// </summary>
        private void BuildControllers()
        {
            var detailPresenter = new StageDetailPresenter(_detailScreenView);
            _stageSelectController = new StageSelectController(_stageTree, detailPresenter, _detailScreenView);
        }

        /// <summary>
        ///     ステージの進行を完了として記録し、後続ノードの接続線アニメーションが完了するまで待機します。
        /// </summary>
        /// <param name="clearedId"> クリアしたステージの ID。</param>
        private async Task CompleteAndAnimateAsync(StageId clearedId)
        {
            _progressService.CompleteStage(clearedId);
            var nextIds = _stageTree.GetNextIds(clearedId);
            for (var i = 0; i < nextIds.Count; i++)
            {
                if (!_nodePresenterMap.TryGetValue(nextIds[i], out var presenter)) { continue; }
                await presenter.TransitionTask;
            }
        }

        /// <summary>
        ///     ノードビューとノードプレゼンターのリソースを解放します。
        /// </summary>
        private void DisposeNodeComponents()
        {
            if (_nodeViews != null)
            {
                for (var i = 0; i < _nodeViews.Count; i++)
                {
                    _nodeViews[i].Dispose();
                }
                _nodeViews.Clear();
            }

            if (_nodePresenters != null)
            {
                for (var i = 0; i < _nodePresenters.Count; i++)
                {
                    _nodePresenters[i].Dispose();
                }
                _nodePresenters.Clear();
                _nodePresenterMap.Clear();
            }
        }

        /// <summary>
        ///     新規クリア済みステージを検出し、後続ノードの解放アニメーションを再生します。
        ///     新規クリアがなければ何も行いません。
        /// </summary>
        private async Task ApplyNewlyClearedStagesAsync(CancellationToken token)
        {
            var newlyClearedIds = _openUseCase.GetNewlyClearedIds();

            for (var i = 0; i < newlyClearedIds.Count; i++)
            {
                if (token.IsCancellationRequested) { return; }
                await CompleteAndAnimateAsync(newlyClearedIds[i]);
            }
        }

    }
}
