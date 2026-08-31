using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーの表示領域を初期フォーカス対象へ合わせるView。
    /// </summary>
    public sealed class SkillTreeViewportView : ISkillTreeFocusViewModel, IDisposable
    {
        /// <summary>
        ///     スキルツリーの表示要素とノード要素を設定する。
        /// </summary>
        /// <param name="rootElement"> UIDocumentのルート要素。 </param>
        /// <param name="nodeElements"> ノードID別の表示要素。 </param>
        public SkillTreeViewportView(
            VisualElement rootElement,
            IReadOnlyDictionary<int, VisualElement> nodeElements)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            if (nodeElements == null)
            {
                throw new ArgumentNullException(nameof(nodeElements));
            }

            _screenRoot = rootElement.Q<VisualElement>(SKILL_TREE_SCREEN_ROOT_NAME)
                ?? throw new InvalidOperationException(
                    $"{SKILL_TREE_SCREEN_ROOT_NAME} が見つかりません。");
            _scrollView = _screenRoot.Q<ScrollView>(SKILL_TREE_CONTAINER_NAME)
                ?? throw new InvalidOperationException(
                    $"{SKILL_TREE_CONTAINER_NAME} が見つかりません。");
            _skillTreeRoot = _scrollView.Q<VisualElement>(SKILL_TREE_ROOT_NAME)
                ?? throw new InvalidOperationException(
                    $"{SKILL_TREE_ROOT_NAME} が見つかりません。");
            _points = _screenRoot.Q<VisualElement>(POINTS_NAME)
                ?? throw new InvalidOperationException($"{POINTS_NAME} が見つかりません。");

            _nodeElements = new Dictionary<int, VisualElement>(nodeElements.Count);
            foreach (KeyValuePair<int, VisualElement> pair in nodeElements)
            {
                _nodeElements.Add(pair.Key, pair.Value);
            }
        }

        /// <summary> 初期フォーカス対象の再取得が必要な時に表示候補IDを通知するイベント。 </summary>
        public event Action<IReadOnlyList<int>> OnFocusTargetsRequested;

        /// <summary>
        ///     現在の表示状態から初期フォーカス処理を要求する。
        /// </summary>
        public void RequestFocus()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SkillTreeViewportView));
            }

            CancelPendingLayout();
            _isFocusRequested = true;
            RequestFocusTargets();
        }

        /// <summary>
        ///     現在の初期フォーカス要求を中止する。
        /// </summary>
        public void CancelFocus()
        {
            if (_isDisposed)
            {
                return;
            }

            CompleteFocusRequest();
        }

        /// <summary>
        ///     初期フォーカス対象のノードIDを設定する。
        /// </summary>
        /// <param name="nodeIds"> 初期フォーカス対象のノードID。 </param>
        public void SetFocusTargets(IReadOnlyList<int> nodeIds)
        {
            if (nodeIds == null)
            {
                throw new ArgumentNullException(nameof(nodeIds));
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SkillTreeViewportView));
            }

            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = null;
            _focusTargetNodeIds = new int[nodeIds.Count];
            for (int i = 0; i < nodeIds.Count; i++)
            {
                _focusTargetNodeIds[i] = nodeIds[i];
            }

            if (_focusTargetNodeIds.Length == 0)
            {
                CompleteFocusRequest();
                return;
            }

            RegisterScreenGeometryCallback();
            SchedulePrepareFocus();
        }

        /// <summary>
        ///     保留中のレイアウト処理とレイアウト変更購読を停止する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            CancelPendingLayout();
            _focusTargetNodeIds = Array.Empty<int>();
            _isFocusRequested = false;
            _isDisposed = true;
        }

        private const string SKILL_TREE_SCREEN_ROOT_NAME = "SkillTreeScreenRoot";
        private const string SKILL_TREE_CONTAINER_NAME = "SkillTreeContainer";
        private const string SKILL_TREE_ROOT_NAME = "SkillTreeRoot";
        private const string POINTS_NAME = "Points";
        private const float FOCUS_POSITION_RATIO = 0.6f;
        private const float POINTS_SAFE_MARGIN = 24.0f;
        private const long LAYOUT_RETRY_DELAY_MILLISECONDS = 16L;

        private readonly VisualElement _screenRoot;
        private readonly ScrollView _scrollView;
        private readonly VisualElement _skillTreeRoot;
        private readonly VisualElement _points;
        private readonly Dictionary<int, VisualElement> _nodeElements;
        private IVisualElementScheduledItem _pendingLayoutItem;
        private int[] _focusTargetNodeIds = Array.Empty<int>();
        private bool _isWaitingForScreenGeometry;
        private bool _isFocusRequested;
        private bool _isDisposed;

        /// <summary>
        ///     画面ルートのレイアウト変更後にフォーカス位置の計算を再開する。
        /// </summary>
        /// <param name="geometryChangedEvent"> 表示領域のレイアウト変更情報。 </param>
        private void HandleScreenGeometryChangedHandler(GeometryChangedEvent geometryChangedEvent)
        {
            if (!IsValidRect(geometryChangedEvent.newRect))
            {
                return;
            }

            RequestFocusTargets();
        }

        /// <summary>
        ///     表示領域のレイアウト確定後にフォーカス位置を計算する処理を予約する。
        /// </summary>
        private void SchedulePrepareFocus()
        {
            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = _screenRoot.schedule.Execute(PrepareFocusAfterLayout);
        }

        /// <summary>
        ///     次のレイアウト更新後に表示候補の取得からやり直す。
        /// </summary>
        private void ScheduleFocusRetry()
        {
            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = _screenRoot.schedule
                .Execute(RequestFocusTargets)
                .StartingIn(LAYOUT_RETRY_DELAY_MILLISECONDS);
        }

        /// <summary>
        ///     対象ノード群と固定表示要素からスクロール量を計算する。
        /// </summary>
        private void PrepareFocusAfterLayout()
        {
            _pendingLayoutItem = null;
            if (_isDisposed || !_isFocusRequested || _focusTargetNodeIds.Length == 0)
            {
                return;
            }

            VisualElement viewport = _scrollView.contentViewport;
            Rect viewportBounds = viewport.worldBound;
            Rect pointsBounds = _points.worldBound;
            if (!IsElementVisible(_scrollView))
            {
                CompleteFocusRequest();
                return;
            }

            if (!IsValidRect(viewportBounds)
                || !IsValidRect(pointsBounds))
            {
                RegisterScreenGeometryCallback();
                ScheduleFocusRetry();
                return;
            }

            if (!TryGetTargetBounds(out Rect targetBounds, out bool isLayoutPending))
            {
                if (isLayoutPending)
                {
                    RegisterScreenGeometryCallback();
                    ScheduleFocusRetry();
                }
                else
                {
                    CompleteFocusRequest();
                }

                return;
            }

            float targetCenterInContent = targetBounds.center.y
                - viewportBounds.yMin
                + _scrollView.scrollOffset.y;
            float defaultFocusY = viewportBounds.height * FOCUS_POSITION_RATIO;
            float pointsTopInViewport = pointsBounds.yMin - viewportBounds.yMin;
            float pointsLimitedFocusY = pointsTopInViewport
                - targetBounds.height * 0.5f
                - POINTS_SAFE_MARGIN;
            float focusY = Mathf.Min(defaultFocusY, pointsLimitedFocusY);
            float scrollOffsetY = targetCenterInContent - focusY;
            CompleteFocusRequest();
            ApplyFocus(scrollOffsetY);
        }

        /// <summary>
        ///     スクロール位置を現在のスクロール範囲へクランプして適用する。
        /// </summary>
        /// <param name="scrollOffsetY"> 適用するY方向のスクロール量。 </param>
        private void ApplyFocus(float scrollOffsetY)
        {
            if (_isDisposed || !IsElementVisible(_scrollView))
            {
                return;
            }

            float lowValue = _scrollView.verticalScroller.lowValue;
            float highValue = _scrollView.verticalScroller.highValue;
            if (!IsFinite(lowValue) || !IsFinite(highValue))
            {
                return;
            }

            Vector2 scrollOffset = _scrollView.scrollOffset;
            scrollOffset.y = Mathf.Clamp(scrollOffsetY, lowValue, highValue);
            _scrollView.scrollOffset = scrollOffset;
        }

        /// <summary>
        ///     表示可能な対象ノード群の外接矩形を取得する。
        /// </summary>
        /// <param name="targetBounds"> 表示可能な対象ノード群の外接矩形。 </param>
        /// <param name="isLayoutPending"> 表示対象のレイアウトが未確定の場合はtrue。 </param>
        /// <returns> 表示可能な対象ノードが存在する場合はtrue。 </returns>
        private bool TryGetTargetBounds(
            out Rect targetBounds,
            out bool isLayoutPending)
        {
            targetBounds = default;
            isLayoutPending = false;
            bool hasTarget = false;
            for (int i = 0; i < _focusTargetNodeIds.Length; i++)
            {
                if (!_nodeElements.TryGetValue(
                        _focusTargetNodeIds[i],
                        out VisualElement nodeElement))
                {
                    continue;
                }

                Rect nodeBounds = nodeElement.worldBound;
                if (!IsValidRect(nodeBounds))
                {
                    isLayoutPending = true;
                    continue;
                }

                if (!hasTarget)
                {
                    targetBounds = nodeBounds;
                    hasTarget = true;
                    continue;
                }

                targetBounds.xMin = Mathf.Min(targetBounds.xMin, nodeBounds.xMin);
                targetBounds.xMax = Mathf.Max(targetBounds.xMax, nodeBounds.xMax);
                targetBounds.yMin = Mathf.Min(targetBounds.yMin, nodeBounds.yMin);
                targetBounds.yMax = Mathf.Max(targetBounds.yMax, nodeBounds.yMax);
            }

            return hasTarget && !isLayoutPending;
        }

        /// <summary>
        ///     ノードが属するSkillTreeRoot直下の子要素を取得する。
        /// </summary>
        /// <param name="nodeElement"> 対象ノード要素。 </param>
        /// <param name="rootChild"> ノードを含むSkillTreeRoot直下の子要素。 </param>
        /// <returns> SkillTreeRoot直下の子要素を取得できた場合はtrue。 </returns>
        private bool TryGetSkillTreeRootChild(
            VisualElement nodeElement,
            out VisualElement rootChild)
        {
            rootChild = nodeElement;
            while (rootChild != null
                && rootChild.parent != null
                && rootChild.parent != _skillTreeRoot)
            {
                rootChild = rootChild.parent;
            }

            return rootChild != null && rootChild.parent == _skillTreeRoot;
        }

        /// <summary>
        ///     SkillTreeRoot直下の表示中コンテナに属するノードIDを取得する。
        /// </summary>
        /// <returns> 初期フォーカス対象になり得るノードID。 </returns>
        private IReadOnlyList<int> GetVisibleCandidateNodeIds()
        {
            List<int> candidateNodeIds = new List<int>();
            foreach (KeyValuePair<int, VisualElement> pair in _nodeElements)
            {
                if (TryGetSkillTreeRootChild(pair.Value, out VisualElement rootChild)
                    && rootChild.resolvedStyle.visibility == Visibility.Visible)
                {
                    candidateNodeIds.Add(pair.Key);
                }
            }

            candidateNodeIds.Sort();
            return candidateNodeIds;
        }

        /// <summary>
        ///     要素自身と祖先が表示状態であるか判定する。
        /// </summary>
        /// <param name="element"> 判定する要素。 </param>
        /// <returns> 解決後の表示状態が有効な場合はtrue。 </returns>
        private static bool IsElementVisible(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None
                    || current.resolvedStyle.visibility != Visibility.Visible)
                {
                    return false;
                }
            }

            return element.panel != null;
        }

        /// <summary>
        ///     レイアウト矩形が座標計算に使用可能か判定する。
        /// </summary>
        /// <param name="rect"> 判定する矩形。 </param>
        /// <returns> 有限の正のサイズを持つ場合はtrue。 </returns>
        private static bool IsValidRect(Rect rect)
        {
            return IsFinite(rect.x)
                && IsFinite(rect.y)
                && IsValidLength(rect.width)
                && IsValidLength(rect.height);
        }

        /// <summary>
        ///     レイアウト値が有限の正数か判定する。
        /// </summary>
        /// <param name="length"> 判定する値。 </param>
        /// <returns> 有限の正数の場合はtrue。 </returns>
        private static bool IsValidLength(float length)
        {
            return IsFinite(length) && length > 0.0f;
        }

        /// <summary>
        ///     値が有限か判定する。
        /// </summary>
        /// <param name="value"> 判定する値。 </param>
        /// <returns> NaNまたは無限大でない場合はtrue。 </returns>
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>
        ///     画面ルートのレイアウト変更を待機する。
        /// </summary>
        private void RegisterScreenGeometryCallback()
        {
            if (_isWaitingForScreenGeometry)
            {
                return;
            }

            _screenRoot.RegisterCallback<GeometryChangedEvent>(
                HandleScreenGeometryChangedHandler);
            _isWaitingForScreenGeometry = true;
        }

        /// <summary>
        ///     保留中のスケジュール処理とレイアウト変更待機を停止する。
        /// </summary>
        private void CancelPendingLayout()
        {
            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = null;
            UnregisterScreenGeometryCallback();
        }

        /// <summary>
        ///     現在の表示候補に対する初期フォーカス対象を要求する。
        /// </summary>
        private void RequestFocusTargets()
        {
            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = null;
            if (_isDisposed || !_isFocusRequested)
            {
                return;
            }

            OnFocusTargetsRequested?.Invoke(GetVisibleCandidateNodeIds());
        }

        /// <summary>
        ///     現在の初期フォーカス要求を完了する。
        /// </summary>
        private void CompleteFocusRequest()
        {
            _pendingLayoutItem?.Pause();
            _pendingLayoutItem = null;
            _focusTargetNodeIds = Array.Empty<int>();
            _isFocusRequested = false;
            UnregisterScreenGeometryCallback();
        }

        /// <summary>
        ///     画面ルートのレイアウト変更待機を解除する。
        /// </summary>
        private void UnregisterScreenGeometryCallback()
        {
            if (!_isWaitingForScreenGeometry)
            {
                return;
            }

            _screenRoot.UnregisterCallback<GeometryChangedEvent>(
                HandleScreenGeometryChangedHandler);
            _isWaitingForScreenGeometry = false;
        }
    }
}
