using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Application.OutGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.StageSelect;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ選択画面の依存を解決するクラス。
    ///     UIBuilder で配置されたノード要素を収集して StageTree と紐付けます。
    /// </summary>
    public sealed class StageSelectInitializer : MonoBehaviour
    {
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
            DisposeNodeViews();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        private void Initialize()
        {
            _outGameUIEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
            if (_outGameUIEvent == null)
            {
                Debug.LogError($"[{nameof(StageSelectInitializer)}] OutGameUIEvent が取得できませんでした。", this);
                return;
            }

            if (_uiDocument == null)
            {
                Debug.LogError($"[{nameof(StageSelectInitializer)}] UIDocument が設定されていません。", this);
                return;
            }

            if (_stageTreeAsset == null)
            {
                Debug.LogError($"[{nameof(StageSelectInitializer)}] StageTreeAsset が設定されていません。", this);
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            // 詳細画面のルート要素を取得する
            VisualElement detailRoot = root.Q<VisualElement>(DETAIL_SCREEN_NAME);
            if (detailRoot == null)
            {
                Debug.LogError($"[{nameof(StageSelectInitializer)}] {DETAIL_SCREEN_NAME} が見つかりませんでした。", this);
                return;
            }

            // --- Domain 層 ---
            StageTree stageTree = _stageTreeAsset.Create();

            // --- Application 層 ---
            StageProgressService progressService = new StageProgressService(stageTree);

            // --- View 層（詳細画面） ---
            _detailScreenView = new StageDetailScreenView(detailRoot, _outGameUIEvent);
            _detailScreenView.HideImmediately();

            // --- View 層（ノード） ---
            // UIBuilder で配置済みの要素を USS クラス名で一括収集する
            // 各ノード要素の name に StageId の文字列値を設定しておくこと
            var nodeElements = root.Query<VisualElement>(className: NODE_USS_CLASS).ToList();
            _nodeViews = new List<StageNodeView>(nodeElements.Count);

            for (var i = 0; i < nodeElements.Count; i++)
            {
                var nodeElement = nodeElements[i];
                var stageIdValue = nodeElement.name;

                if (string.IsNullOrEmpty(stageIdValue))
                {
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] USS クラス '{NODE_USS_CLASS}' の要素 (index:{i}) に name が設定されていません。", this);
                    continue;
                }

                if (!stageTree.TryGetNode(new StageId(stageIdValue), out var node))
                {
                    Debug.LogWarning(
                        $"[{nameof(StageSelectInitializer)}] StageId '{stageIdValue}' に対応するノードが StageTree に存在しません。", this);
                    continue;
                }

                var nodeView = new StageNodeView(nodeElement, stageIdValue, _outGameUIEvent);

                // ノードの初期状態を反映する
                var nodePresenter = new StageNodePresenter(nodeView);
                nodePresenter.Push(node);

                _nodeViews.Add(nodeView);
            }

            // --- Adaptor 層 ---
            StageDetailPresenter detailPresenter = new StageDetailPresenter(_detailScreenView);
            _stageSelectController = new StageSelectController(stageTree, detailPresenter, _detailScreenView);

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
        }

        /// <summary>
        ///     ステージノードが選択されたときのイベントハンドラ。
        /// </summary>
        /// <param name="stageIdValue">選択されたステージ ID の文字列値。</param>
        private void HandleStageNodeSelected(string stageIdValue)
        {
            _stageSelectController.OnStageNodeSelected(stageIdValue, _cts.Token);
        }

        /// <summary>
        ///     ステージ詳細画面を閉じるイベントハンドラ。
        /// </summary>
        private void HandleStageDetailClosed()
        {
            _detailScreenView.Hide(_cts.Token);
        }

        /// <summary>
        ///     ステージ選択画面が閉じられたときのイベントハンドラ。
        ///     詳細画面が表示中の場合は即座に閉じます。
        /// </summary>
        private void HandleScreenClosed()
        {
            _detailScreenView.HideImmediately();
        }

        /// <summary>
        ///     ノードビューのリソースを解放します。
        /// </summary>
        private void DisposeNodeViews()
        {
            if (_nodeViews == null) { return; }

            for (var i = 0; i < _nodeViews.Count; i++)
            {
                _nodeViews[i].Dispose();
            }

            _nodeViews.Clear();
        }

        /// <summary> ノードとして収集する要素のUSSクラス名。 </summary>
        private const string NODE_USS_CLASS = "stage-node";
        /// <summary> ステージ詳細画面のルート要素名。 </summary>
        private const string DETAIL_SCREEN_NAME = "StageDetailContainer";

        [SerializeField, Tooltip("ステージ選択画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("ステージツリーの定義アセットです。")]
        private StageTreeAsset _stageTreeAsset;

        private OutGameUIEvent _outGameUIEvent;
        private StageSelectController _stageSelectController;
        private StageDetailScreenView _detailScreenView;
        private List<StageNodeView> _nodeViews;
        private CancellationTokenSource _cts;
        private bool _isInitialized;
    }
}
