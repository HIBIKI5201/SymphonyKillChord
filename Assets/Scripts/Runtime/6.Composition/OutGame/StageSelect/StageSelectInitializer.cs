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
    ///     StageTreeから作戦画面のノードと接続線を生成して各機能を紐付ける。
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
        /// <summary> 接続線塗りつぶし要素のUSSクラス名。 </summary>
        private const string CONNECTION_FILL_USS_CLASS = "stage-connection__fill";
        /// <summary> 通常ステージノードのUSSクラス名。 </summary>
        private const string STAGE_NODE_USS_CLASS = "stage-node--stage";
        /// <summary> バトルステージノードのUSSクラス名。 </summary>
        private const string BATTLE_NODE_USS_CLASS = "stage-node--boss";
        /// <summary> ステージノードラベルのUSSクラス名。 </summary>
        private const string NODE_LABEL_USS_CLASS = "stage-node__label";
        /// <summary> 作戦マップScrollView要素名。 </summary>
        private const string STAGE_MAP_SCROLL_VIEW_NAME = "StageNodeRoot";
        /// <summary> 作戦マップ内容要素名。 </summary>
        private const string STAGE_MAP_CONTENT_NAME = "MainRow";
        /// <summary> 自動生成する作戦マップ描画領域名。 </summary>
        private const string STAGE_MAP_CANVAS_NAME = "StageMapCanvas";
        /// <summary> 接続線塗りつぶし要素名。 </summary>
        private const string CONNECTION_FILL_NAME = "ConnectionFill";
        /// <summary> ステージ詳細画面のルート要素名。 </summary>
        private const string DETAIL_SCREEN_NAME = "StageDetailContainer";
        /// <summary> ノードの一辺の長さ。 </summary>
        private const float NODE_SIZE = 100.0f;
        /// <summary> 列間の中心距離。 </summary>
        private const float HORIZONTAL_SPACING = 220.0f;
        /// <summary> 行間の中心距離。 </summary>
        private const float VERTICAL_SPACING = 160.0f;
        /// <summary> マップ内容の周囲余白。 </summary>
        private const float MAP_PADDING = 40.0f;
        /// <summary> 接続線の太さ。 </summary>
        private const float CONNECTION_THICKNESS = 14.0f;

        [SerializeField, Tooltip("ステージ選択画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField, SourceDataAddress, Tooltip("ステージツリー定義アセットの Addressables キーです。")]
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
        private PendingNodeTransitionState _pendingNodeTransitionState;
        private BattleSortieSelectionService _battleSortieSelectionService;
        private StageTreeAsset _loadedStageTreeAsset;
        private SaveData _loadedSaveData;
        private ScrollView _stageMapScrollView;
        private VisualElement _stageMapContent;
        private VisualElement _stageMapCanvas;
        private float _stageMapCanvasHeight;
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
            TryExecutePendingNodeTransitionAfterReturn();
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

            ReserveNodeTransitionChain(stageDefinition);
            if (stageDefinition is BattleStageDefinition battleStageDefinition)
            {
                _selectedScenarioState.Clear();
                if (!TryPrepareBattleSortie(battleStageDefinition))
                {
                    _pendingNodeTransitionState?.Clear();
                    return;
                }
            }
            else if (stageDefinition is ScenarioStageDefinition scenarioStageDefinition)
            {
                _selectedBattleStageState.Clear();
                _selectedMissionState.Clear();

                _selectedScenarioState.SelectScenario(scenarioStageDefinition);
            }

            bool requested = await _outGameSortieController.RequestSortieAsync(
                stageDefinition.StageType,
                _currentSceneName,
                stageDefinition.TargetSceneName,
                _cts.Token);
            if (!requested)
            {
                _pendingNodeTransitionState?.Clear();
            }
        }

        /// <summary>
        ///     バトルステージとミッションの選択状態を構築します。
        /// </summary>
        /// <param name="stageDefinition"> 出撃するステージ定義です。 </param>
        /// <returns> 出撃準備に成功した場合はtrueです。 </returns>
        private bool TryPrepareBattleSortie(BattleStageDefinition stageDefinition)
        {
            if (_battleSortieSelectionService == null)
            {
                return false;
            }

            return _battleSortieSelectionService.TryPrepareBattleSortie(stageDefinition, _currentSceneName);
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
        ///     作戦マップの表示領域変更に合わせて縦中央位置を更新する。
        /// </summary>
        /// <param name="evt"> 変更後の表示領域情報。</param>
        private void StageMapViewportGeometryChangedHandler(GeometryChangedEvent evt)
        {
            UpdateStageMapVerticalAlignment(evt.newRect.height);
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

            if (!ServiceLocator.TryGetInstance(out _pendingNodeTransitionState))
            {
                _pendingNodeTransitionState = new PendingNodeTransitionState();
                ServiceLocator.RegisterInstance(_pendingNodeTransitionState);
            }

            _battleSortieSelectionService = new BattleSortieSelectionService();
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
            var layoutBuilder = new StageMapLayoutBuilder();
            IReadOnlyDictionary<StageId, StageMapNodePosition> nodePositions;
            try
            {
                nodePositions = layoutBuilder.Build(_stageTree, out int rootCount);
#if UNITY_EDITOR
                if (rootCount > 1)
                {
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] 起点ステージが{rootCount}個あります。" +
                        "作戦画面の左端に複数配置します。",
                        this);
                }
#endif
            }
            catch (System.InvalidOperationException exception)
            {
                Debug.LogError($"[{nameof(StageSelectInitializer)}] {exception.Message}", this);
                return false;
            }

            if (!TryBuildStageMapElements(
                    root,
                    nodePositions,
                    out Dictionary<StageId, VisualElement> nodeElementMap,
                    out Dictionary<StageId, List<IStageConnectionViewModel>> connectionViewMap))
            {
                return false;
            }

            BuildNodeComponents(nodeElementMap, connectionViewMap);

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
            UnregisterStageMapGeometryCallback();
            _cts?.Cancel();
            DisposeNodeComponents();
            _cts?.Dispose();
            _cts = null;
            _stageTreeAssetKey.ReleaseLoadedAsset(this);
            _loadedStageTreeAsset = null;
            _loadedSaveData = null;
            _stageMapScrollView = null;
            _stageMapContent = null;
            _stageMapCanvas = null;
            _stageMapCanvasHeight = 0.0f;
            _battleSortieSelectionService = null;
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
        ///     StageTreeの配置情報からノードと接続線のVisualElementを生成する。
        /// </summary>
        /// <param name="root"> 作戦画面のルート要素。</param>
        /// <param name="nodePositions"> ステージID別のグリッド位置。</param>
        /// <param name="nodeElementMap"> 生成したノード要素。</param>
        /// <param name="connectionViewMap"> 接続先別の接続線ViewModel一覧。</param>
        /// <returns> 生成に成功した場合はtrue。</returns>
        private bool TryBuildStageMapElements(
            VisualElement root,
            IReadOnlyDictionary<StageId, StageMapNodePosition> nodePositions,
            out Dictionary<StageId, VisualElement> nodeElementMap,
            out Dictionary<StageId, List<IStageConnectionViewModel>> connectionViewMap)
        {
            nodeElementMap = new Dictionary<StageId, VisualElement>(_stageTree.Nodes.Count);
            connectionViewMap = new Dictionary<StageId, List<IStageConnectionViewModel>>();
            ScrollView mapScrollView = root.Q<ScrollView>(STAGE_MAP_SCROLL_VIEW_NAME);
            VisualElement mapContent = root.Q<VisualElement>(STAGE_MAP_CONTENT_NAME);
            if (mapScrollView == null || mapContent == null)
            {
                Debug.LogError(
                    $"[{nameof(StageSelectInitializer)}] 作戦マップのScrollViewまたは内容要素が見つかりませんでした。",
                    this);
                return false;
            }

            UnregisterStageMapGeometryCallback();
            mapContent.Clear();
            var mapCanvas = new VisualElement
            {
                name = STAGE_MAP_CANVAS_NAME,
                pickingMode = PickingMode.Position,
            };
            mapContent.Add(mapCanvas);
            Dictionary<StageId, Vector2> nodeCenters = BuildNodeCenterMap(
                mapCanvas,
                nodePositions,
                out float canvasWidth,
                out float canvasHeight);
            mapContent.style.position = Position.Relative;
            mapContent.style.width = canvasWidth;
            mapContent.style.flexShrink = 0.0f;
            BuildConnectionElements(mapCanvas, nodeCenters, connectionViewMap);
            BuildNodeElements(mapCanvas, nodeCenters, nodeElementMap);

            _stageMapScrollView = mapScrollView;
            _stageMapContent = mapContent;
            _stageMapCanvas = mapCanvas;
            _stageMapCanvasHeight = canvasHeight;
            _stageMapScrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(
                StageMapViewportGeometryChangedHandler);
            UpdateStageMapVerticalAlignment(
                _stageMapScrollView.contentViewport.resolvedStyle.height);
            _stageMapScrollView.schedule.Execute(UpdateStageMapVerticalAlignmentAfterLayout);
            return true;
        }

        /// <summary>
        ///     UIレイアウト確定後の表示領域サイズを使用して縦中央位置を更新する。
        /// </summary>
        private void UpdateStageMapVerticalAlignmentAfterLayout()
        {
            if (_stageMapScrollView == null)
            {
                return;
            }

            float viewportHeight = _stageMapScrollView.contentViewport.layout.height;
            if (!IsValidLayoutLength(viewportHeight))
            {
                viewportHeight = _stageMapScrollView.contentViewport.resolvedStyle.height;
            }
            if (!IsValidLayoutLength(viewportHeight))
            {
                viewportHeight = _stageMapScrollView.layout.height;
            }

            UpdateStageMapVerticalAlignment(viewportHeight);
        }

        /// <summary>
        ///     作戦マップCanvasを表示領域の縦中央へ配置する。
        /// </summary>
        /// <param name="viewportHeight"> ScrollView表示領域の高さ。</param>
        private void UpdateStageMapVerticalAlignment(float viewportHeight)
        {
            if (_stageMapContent == null || _stageMapCanvas == null)
            {
                return;
            }

            if (!IsValidLayoutLength(viewportHeight))
            {
                viewportHeight = _stageMapCanvasHeight;
            }

            float contentHeight = Mathf.Max(_stageMapCanvasHeight, viewportHeight);
            float canvasTop = (contentHeight - _stageMapCanvasHeight) * 0.5f;
            _stageMapContent.style.height = contentHeight;
            _stageMapCanvas.style.top = canvasTop;
        }

        /// <summary>
        ///     UIレイアウトから取得した長さが配置計算に使用可能か判定する。
        /// </summary>
        /// <param name="length"> 判定する長さ。</param>
        /// <returns> 有限の正数である場合はtrue。</returns>
        private static bool IsValidLayoutLength(float length)
        {
            return !float.IsNaN(length)
                && !float.IsInfinity(length)
                && length > 0.0f;
        }

        /// <summary>
        ///     作戦マップ表示領域のサイズ変更購読を解除する。
        /// </summary>
        private void UnregisterStageMapGeometryCallback()
        {
            _stageMapScrollView?.contentViewport?.UnregisterCallback<GeometryChangedEvent>(
                StageMapViewportGeometryChangedHandler);
        }

        /// <summary>
        ///     StageNodeViewとStageNodePresenterを生成して各リストへ登録する。
        /// </summary>
        /// <param name="nodeElementMap"> ステージID別のノード要素。</param>
        /// <param name="connectionViewMap"> 接続先別の接続線ViewModel一覧。</param>
        private void BuildNodeComponents(
            Dictionary<StageId, VisualElement> nodeElementMap,
            Dictionary<StageId, List<IStageConnectionViewModel>> connectionViewMap)
        {
            _nodeViews = new List<StageNodeView>(_stageTree.Nodes.Count);
            _nodePresenters = new List<StageNodePresenter>(_stageTree.Nodes.Count);
            _nodePresenterMap = new Dictionary<StageId, StageNodePresenter>(_stageTree.Nodes.Count);

            // ノードのアニメーションを順番に再生するためのシーケンサーを生成する
            var sequencer = new StageNodeAnimationSequencer();

            for (int i = 0; i < _stageTree.Nodes.Count; i++)
            {
                StageNode node = _stageTree.Nodes[i];
                StageId stageId = node.Id;
                VisualElement nodeElement = nodeElementMap[stageId];
                var nodeView = new StageNodeView(nodeElement, stageId.Value, _outGameUIEvent);

                // このノードへの接続線View一覧を取得する（存在しない場合はnull）。
                connectionViewMap.TryGetValue(stageId, out List<IStageConnectionViewModel> incomingConnectionViews);

                var nodePresenter = new StageNodePresenter(
                    node,
                    nodeView,
                    incomingConnectionViews,
                    sequencer);

                _nodeViews.Add(nodeView);
                _nodePresenters.Add(nodePresenter);
                // ID で引けるようにマップへも登録する
                _nodePresenterMap.Add(stageId, nodePresenter);
            }
        }

        /// <summary>
        ///     グリッド位置を画面座標へ変換してマップ内容サイズを設定する。
        /// </summary>
        /// <param name="mapCanvas"> ノードと接続線を格納する描画領域。</param>
        /// <param name="nodePositions"> ステージID別のグリッド位置。</param>
        /// <param name="contentWidth"> 描画領域の幅。</param>
        /// <param name="contentHeight"> 描画領域の高さ。</param>
        /// <returns> ステージID別のノード中心座標。</returns>
        private static Dictionary<StageId, Vector2> BuildNodeCenterMap(
            VisualElement mapCanvas,
            IReadOnlyDictionary<StageId, StageMapNodePosition> nodePositions,
            out float contentWidth,
            out float contentHeight)
        {
            Dictionary<int, int> columnCounts = new();
            int maxColumn = 0;
            int maxRowCount = 1;
            foreach (KeyValuePair<StageId, StageMapNodePosition> pair in nodePositions)
            {
                int rowCount = pair.Value.Row + 1;
                if (!columnCounts.TryGetValue(pair.Value.Column, out int currentRowCount)
                    || rowCount > currentRowCount)
                {
                    columnCounts[pair.Value.Column] = rowCount;
                }
                maxColumn = Mathf.Max(maxColumn, pair.Value.Column);
                maxRowCount = Mathf.Max(maxRowCount, rowCount);
            }

            contentWidth = MAP_PADDING * 2.0f + NODE_SIZE + maxColumn * HORIZONTAL_SPACING;
            contentHeight = MAP_PADDING * 2.0f + NODE_SIZE + (maxRowCount - 1) * VERTICAL_SPACING;
            mapCanvas.style.position = Position.Absolute;
            mapCanvas.style.left = 0.0f;
            mapCanvas.style.top = 0.0f;
            mapCanvas.style.width = contentWidth;
            mapCanvas.style.height = contentHeight;

            Dictionary<StageId, Vector2> nodeCenters = new(nodePositions.Count);
            foreach (KeyValuePair<StageId, StageMapNodePosition> pair in nodePositions)
            {
                int columnCount = columnCounts[pair.Value.Column];
                float columnOffset = (maxRowCount - columnCount) * VERTICAL_SPACING * 0.5f;
                float centerX = MAP_PADDING + NODE_SIZE * 0.5f
                    + pair.Value.Column * HORIZONTAL_SPACING;
                float centerY = MAP_PADDING + NODE_SIZE * 0.5f + columnOffset
                    + pair.Value.Row * VERTICAL_SPACING;
                nodeCenters.Add(pair.Key, new Vector2(centerX, centerY));
            }

            return nodeCenters;
        }

        /// <summary>
        ///     Bindごとの接続線要素を生成する。
        /// </summary>
        /// <param name="mapContent"> 接続線を格納する要素。</param>
        /// <param name="nodeCenters"> ステージID別のノード中心座標。</param>
        /// <param name="connectionViewMap"> 接続先別の接続線ViewModel一覧。</param>
        private void BuildConnectionElements(
            VisualElement mapContent,
            Dictionary<StageId, Vector2> nodeCenters,
            Dictionary<StageId, List<IStageConnectionViewModel>> connectionViewMap)
        {
            for (int i = 0; i < _stageTree.Connections.Count; i++)
            {
                StageNodeConnection connection = _stageTree.Connections[i];
                Vector2 fromPosition = nodeCenters[connection.FromStageId];
                Vector2 toPosition = nodeCenters[connection.ToStageId];
                Vector2 direction = toPosition - fromPosition;

                var connectionElement = new VisualElement
                {
                    name = $"connection-{connection.FromStageId.Value}-{connection.ToStageId.Value}",
                    pickingMode = PickingMode.Ignore,
                };
                connectionElement.AddToClassList(CONNECTION_USS_CLASS);
                connectionElement.style.position = Position.Absolute;
                connectionElement.style.left = fromPosition.x;
                connectionElement.style.top = fromPosition.y - CONNECTION_THICKNESS * 0.5f;
                connectionElement.style.width = direction.magnitude;
                connectionElement.style.height = CONNECTION_THICKNESS;
                connectionElement.style.transformOrigin = new TransformOrigin(
                    Length.Percent(0.0f),
                    Length.Percent(50.0f));
                connectionElement.style.rotate = new Rotate(
                    new Angle(
                        Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg,
                        AngleUnit.Degree));

                var fillElement = new VisualElement
                {
                    name = CONNECTION_FILL_NAME,
                    pickingMode = PickingMode.Ignore,
                };
                fillElement.AddToClassList(CONNECTION_FILL_USS_CLASS);
                connectionElement.Add(fillElement);
                mapContent.Add(connectionElement);

                if (!connectionViewMap.TryGetValue(
                        connection.ToStageId,
                        out List<IStageConnectionViewModel> incomingConnections))
                {
                    incomingConnections = new List<IStageConnectionViewModel>();
                    connectionViewMap.Add(connection.ToStageId, incomingConnections);
                }

                incomingConnections.Add(new StageNodeConnectionView(connectionElement));
            }
        }

        /// <summary>
        ///     ステージごとのノード要素を生成する。
        /// </summary>
        /// <param name="mapContent"> ノードを格納する要素。</param>
        /// <param name="nodeCenters"> ステージID別のノード中心座標。</param>
        /// <param name="nodeElementMap"> 生成したノード要素。</param>
        private void BuildNodeElements(
            VisualElement mapContent,
            Dictionary<StageId, Vector2> nodeCenters,
            Dictionary<StageId, VisualElement> nodeElementMap)
        {
            for (int i = 0; i < _stageTree.Nodes.Count; i++)
            {
                StageNode node = _stageTree.Nodes[i];
                Vector2 center = nodeCenters[node.Id];
                var nodeElement = new Button
                {
                    name = $"stage-{node.Id.Value}",
                    text = string.Empty,
                };
                nodeElement.AddToClassList("button");
                nodeElement.AddToClassList(NODE_USS_CLASS);
                nodeElement.AddToClassList(
                    node.Definition.StageType == StageType.Battle
                        ? BATTLE_NODE_USS_CLASS
                        : STAGE_NODE_USS_CLASS);
                nodeElement.style.position = Position.Absolute;
                nodeElement.style.left = center.x - NODE_SIZE * 0.5f;
                nodeElement.style.top = center.y - NODE_SIZE * 0.5f;
                nodeElement.style.width = NODE_SIZE;
                nodeElement.style.height = NODE_SIZE;

                var label = new Label(node.Definition.StageName)
                {
                    pickingMode = PickingMode.Ignore,
                };
                label.AddToClassList(NODE_LABEL_USS_CLASS);
                nodeElement.Add(label);
                mapContent.Add(nodeElement);
                nodeElementMap.Add(node.Id, nodeElement);
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
        ///     現在のステージから連続する自動遷移を予約する。
        /// </summary>
        /// <param name="stageDefinition"> 出撃対象のステージ定義です。 </param>
        private void ReserveNodeTransitionChain(StageDefinition stageDefinition)
        {
            if (_pendingNodeTransitionState == null)
            {
                return;
            }

            _pendingNodeTransitionState.Clear();
            if (stageDefinition == null)
            {
                return;
            }

            HashSet<StageId> visitedStageIds = new();
            StageDefinition currentStageDefinition = stageDefinition;
            while (_stageTree.TryGetAutoAdvanceTarget(
                       currentStageDefinition.StageId,
                       out StageDefinition targetStageDefinition))
            {
                if (!visitedStageIds.Add(currentStageDefinition.StageId))
                {
#if UNITY_EDITOR
                    Debug.LogError(
                        $"[{nameof(StageSelectInitializer)}] 自動遷移に循環があります。" +
                        $"StageId: {currentStageDefinition.StageId.Value}",
                        this);
#endif
                    _pendingNodeTransitionState.Clear();
                    return;
                }

                _pendingNodeTransitionState.Reserve(new PendingNodeTransition(
                    currentStageDefinition.StageId,
                    targetStageDefinition,
                    _currentSceneName));
                currentStageDefinition = targetStageDefinition;
            }
        }

        /// <summary>
        ///     バトルからホームへ戻った後の予約済み自動遷移を実行する。
        /// </summary>
        private async void TryExecutePendingNodeTransitionAfterReturn()
        {
            if (_pendingNodeTransitionState == null
                || !_pendingNodeTransitionState.HasPending)
            {
                return;
            }

            if (!_pendingNodeTransitionState.TryConsumeCompleted(
                    out PendingNodeTransition pendingNodeTransition))
            {
                _pendingNodeTransitionState.Clear();
                return;
            }

            try
            {
                if (!await ExecutePendingNodeTransitionAsync(pendingNodeTransition))
                {
                    _pendingNodeTransitionState.Clear();
                }
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (System.Exception exception)
            {
                _pendingNodeTransitionState.Clear();
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     予約済み遷移の対象ステージを自動開始する。
        /// </summary>
        /// <param name="pendingNodeTransition"> 実行する予約済み遷移。</param>
        /// <returns> 開始要求に成功した場合はtrue。</returns>
        private async Task<bool> ExecutePendingNodeTransitionAsync(
            PendingNodeTransition pendingNodeTransition)
        {
            if (pendingNodeTransition.TargetStageDefinition
                is BattleStageDefinition battleStageDefinition)
            {
                _selectedScenarioState.Clear();
                if (!_battleSortieSelectionService.TryPrepareBattleSortie(
                        battleStageDefinition,
                        pendingNodeTransition.ReturnSceneName))
                {
                    return false;
                }

                return _outGameSortieController.RequestImmediateBattleSortie(
                    battleStageDefinition.TargetSceneName);
            }

            if (pendingNodeTransition.TargetStageDefinition
                is ScenarioStageDefinition scenarioStageDefinition)
            {
                _selectedBattleStageState.Clear();
                _selectedMissionState.Clear();
                _selectedScenarioState.SelectScenario(scenarioStageDefinition);
                return await _outGameSortieController.RequestSortieAsync(
                    StageType.Scenario,
                    _currentSceneName,
                    scenarioStageDefinition.TargetSceneName,
                    _cts.Token);
            }

            return false;
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
