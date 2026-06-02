using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Application.OutGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.StageSelect;
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
    public sealed class StageSelectInitializer : MonoBehaviour
    {
        /// <summary> ノード要素のUSSクラス名。 </summary>
        private const string NODE_USS_CLASS = "stage-node";
        /// <summary> 接続線要素のUSSクラス名。 </summary>
        private const string CONNECTION_USS_CLASS = "stage-connection";
        /// <summary> 接続線要素のname形式。 </summary>
        private const string CONNECTION_NAME_FORMAT = "{fromId}-{toId}";
        /// <summary> ステージ詳細画面のルート要素名。 </summary>
        private const string DETAIL_SCREEN_NAME = "StageDetailContainer";

        [SerializeField, Tooltip("ステージ選択画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("ステージツリーの定義アセットです。")]
        private StageTreeAsset _stageTreeAsset;

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

        /// <summary>
        ///     初期化を行います。
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        ///     イベントを購読します。
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
            DisposeNodeComponents();
            _cts?.Cancel();
            _cts?.Dispose();
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
        ///     システムを構築します。
        /// </summary>
        private void Initialize()
        {
            _outGameUIEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
            if (_outGameUIEvent == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return;
            }

            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] UIDocument が設定されていません。", this);
#endif
                return;
            }

            if (_stageTreeAsset == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] StageTreeAsset が設定されていません。", this);
#endif
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            // 詳細画面のルート要素を取得する
            VisualElement detailRoot = root.Q<VisualElement>(DETAIL_SCREEN_NAME);
            if (detailRoot == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(StageSelectInitializer)}] {DETAIL_SCREEN_NAME} が見つかりませんでした。", this);
#endif
                return;
            }

            // --- Domain 層 ---
            _stageTree = _stageTreeAsset.Create();

            // --- Application 層 ---
            _progressService = new StageProgressService(_stageTree);

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
        }

        /// <summary>
        ///     UIイベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized) { return; }
            _outGameUIEvent.OnStageNodeSelected += HandleStageNodeSelected;
            _outGameUIEvent.OnStageDetailClosed += HandleStageDetailClosed;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosed;
            _outGameUIEvent.OnStageCleared += HandleStageCleared;
        }

        /// <summary>
        ///     UIイベントの購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isInitialized) { return; }
            _outGameUIEvent.OnStageNodeSelected -= HandleStageNodeSelected;
            _outGameUIEvent.OnStageDetailClosed -= HandleStageDetailClosed;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosed;
            _outGameUIEvent.OnStageCleared -= HandleStageCleared;
        }

        /// <summary>
        ///     接続線 VisualElement を収集し、ToStageId をキーとした Map を構築します。
        /// </summary>
        /// <param name="root"> 検索対象のルート VisualElement。</param>
        /// <returns> ToStageId → StageNodeConnectionView の辞書。</returns>
        private Dictionary<StageId, StageNodeConnectionView> BuildConnectionViewMap(VisualElement root)
        {
            // 接続線要素の name は "{fromId}-{toId}" 形式で設定しておくこと
            // 例）1-2
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

                // ToStageId をキーにして接続線 View を管理する
                if (!int.TryParse(parts[1], out var toStageIdInt))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] 接続線要素 '{element.name}' の ToStageId を int に変換できませんでした。", this);
#endif
                    continue;
                }

                var toStageId = new StageId(toStageIdInt);
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

            for (var i = 0; i < nodeElements.Count; i++)
            {
                var nodeElement = nodeElements[i];
                var stageIdValue = nodeElement.name;

                // ステージノードの VisualElement の name には
                // 対応する StageId を int 形式で設定しておくことを前提とする。
                if (string.IsNullOrEmpty(stageIdValue))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] USS クラス '{NODE_USS_CLASS}' の要素 (index:{i}) に name が設定されていません。", this);
#endif
                    continue;
                }

                if (!int.TryParse(stageIdValue, out var stageIdInt))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] ノード要素 '{stageIdValue}' の name を int に変換できませんでした。", this);
#endif
                    continue;
                }

                var stageId = new StageId(stageIdInt);
                if (!_stageTree.TryGetNode(stageId, out var node))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] StageId '{stageIdInt}' に対応するノードが StageTree に存在しません。", this);
#endif
                    continue;
                }

                var nodeView = new StageNodeView(nodeElement, stageIdInt, _outGameUIEvent);

                // このノードへの接続線Viewを取得する（存在しない場合は null）
                connectionViewMap.TryGetValue(stageId, out var incomingConnectionView);

                var nodePresenter = new StageNodePresenter(node, nodeView, incomingConnectionView);

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
    }
}
