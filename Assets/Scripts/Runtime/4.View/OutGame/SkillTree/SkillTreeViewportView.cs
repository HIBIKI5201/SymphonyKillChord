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

            EnforceScrollerHidden();
        }

        /// <summary>
        ///     縦横のスクロールバーを常に非表示にする。UXML側の設定だけに頼らずコードからも強制する。
        /// </summary>
        private void EnforceScrollerHidden()
        {
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.verticalScroller.style.display = DisplayStyle.None;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.horizontalScroller.style.display = DisplayStyle.None;
        }

        /// <summary> 初期フォーカス対象の再取得が必要な時に表示候補IDを通知するイベント。 </summary>
        public event Action<IReadOnlyList<int>> OnFocusTargetsRequested;

        /// <summary>
        ///     現在表示されているスクロール領域の画面上の矩形を取得します。
        ///     ノード間のコントローラー移動先を、画面内に見えている範囲だけに絞り込むために使用します。
        /// </summary>
        public Rect ViewportWorldBound => _scrollView.contentViewport.worldBound;

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
        ///     選択したノードへツリー全体をズームイン(拡大+中央寄せ)する。
        ///     作戦画面(StageSelectInitializer.ZoomMapToNode)と同じ考え方で、
        ///     呼ぶたびに拡大の基点をそのノードの位置へ設定する。
        /// </summary>
        /// <param name="nodeId"> ズーム対象のノードID。 </param>
        public void FocusOnNode(int nodeId)
        {
            if (_isDisposed
                || !_nodeElements.TryGetValue(nodeId, out VisualElement nodeElement))
            {
                return;
            }

            Rect nodeWorldBound = nodeElement.worldBound;
            Rect viewportBounds = _scrollView.contentViewport.worldBound;
            if (!IsValidRect(nodeWorldBound) || !IsValidRect(viewportBounds))
            {
                return;
            }

            Vector2 nodeLocalCenter = _skillTreeRoot.WorldToLocal(nodeWorldBound.center);

            // 基点切替(A→B)の瞬間、スケールが掛かったままノードB自身の画面上位置も
            // Δ=(B-A)×(1-旧倍率)だけ瞬時にジャンプする。切替前に読んだworldBoundは
            // このジャンプ後の位置を反映していないため、ここで予測して補正する。
            // 未ズーム状態からの初回フォーカスも、基準倍率(BASE_SCALE)かつ
            // 中央(USSのtransform-origin: 50% 50%)を旧基点として同じジャンプが起きるため、
            // 常に補正する。
            Vector2 oldFocusLocalCenter = _isZoomed
                ? _focusedNodeLocalCenter
                : _skillTreeRoot.WorldToLocal(_skillTreeRoot.worldBound.center);
            Vector2 delta = (nodeLocalCenter - oldFocusLocalCenter) * (1f - _currentScale);
            Vector2 predictedCenter = nodeWorldBound.center + delta;

            EnforceScrollerHidden();

            // FocusOnNodeRangeが設定した動的なインラインscaleを解除し、USSクラス側の倍率へ戻す。
            _skillTreeRoot.style.scale = StyleKeyword.Null;
            _skillTreeRoot.style.transformOrigin =
                new TransformOrigin(nodeLocalCenter.x, nodeLocalCenter.y);
            _skillTreeRoot.AddToClassList(ZOOMED_USS_CLASS);
            _focusedNodeLocalCenter = nodeLocalCenter;
            _currentScale = ZOOM_SCALE;

            if (!_isZoomed)
            {
                _scrollOffsetYBeforeZoom = _scrollView.scrollOffset.y;
                _scrollOffsetXBeforeZoom = _scrollView.scrollOffset.x;
                _isZoomed = true;
            }

            float targetCenterInContentY = predictedCenter.y
                - viewportBounds.yMin
                + _scrollView.scrollOffset.y;
            float focusY = viewportBounds.height * 0.5f;
            float targetCenterInContentX = predictedCenter.x
                - viewportBounds.xMin
                + _scrollView.scrollOffset.x;
            float focusX = viewportBounds.width * 0.5f;
            AnimateScrollOffsetTo(new Vector2(
                ClampScrollOffsetX(targetCenterInContentX - focusX),
                ClampScrollOffsetY(targetCenterInContentY - focusY)));
        }

        /// <summary>
        ///     指定要素が現在のビューポート外(または余白未満)にある場合のみ、
        ///     それが見える位置まで最小限スクロールする。ズームや倍率、基点は変更しない。
        ///     <para>
        ///         コントローラーでのノード間移動時、接続先ノードが画面外にあっても
        ///         フォーカス自体は移せるようにしているため、その移動に追従して
        ///         画面を自動でスクロールさせる用途に使う。
        ///     </para>
        /// </summary>
        /// <param name="element"> 可視範囲に収めたい要素。スキルツリー外の要素は無視される。 </param>
        public void EnsureVisible(VisualElement element)
        {
            if (_isDisposed || element == null)
            {
                return;
            }

            // 要素と表示範囲の大きさがまだ決まっていない場合は何もしない。
            Rect elementBounds = element.worldBound;
            Rect viewportBounds = _scrollView.contentViewport.worldBound;
            if (!IsValidRect(elementBounds) || !IsValidRect(viewportBounds))
            {
                return;
            }

            // 要素が余白込みで表示範囲からはみ出している分だけ、スクロール位置をずらす。
            float scrollOffsetX = _scrollView.scrollOffset.x;
            float scrollOffsetY = _scrollView.scrollOffset.y;

            if (elementBounds.yMin < viewportBounds.yMin + ENSURE_VISIBLE_MARGIN)
            {
                scrollOffsetY -= (viewportBounds.yMin + ENSURE_VISIBLE_MARGIN) - elementBounds.yMin;
            }
            else if (elementBounds.yMax > viewportBounds.yMax - ENSURE_VISIBLE_MARGIN)
            {
                scrollOffsetY += elementBounds.yMax - (viewportBounds.yMax - ENSURE_VISIBLE_MARGIN);
            }

            if (elementBounds.xMin < viewportBounds.xMin + ENSURE_VISIBLE_MARGIN)
            {
                scrollOffsetX -= (viewportBounds.xMin + ENSURE_VISIBLE_MARGIN) - elementBounds.xMin;
            }
            else if (elementBounds.xMax > viewportBounds.xMax - ENSURE_VISIBLE_MARGIN)
            {
                scrollOffsetX += elementBounds.xMax - (viewportBounds.xMax - ENSURE_VISIBLE_MARGIN);
            }

            // スクロールできる範囲に収め、ほとんど動かない場合は何もしない。
            Vector2 targetOffset = new Vector2(
                ClampScrollOffsetX(scrollOffsetX),
                ClampScrollOffsetY(scrollOffsetY));
            if ((targetOffset - _scrollView.scrollOffset).sqrMagnitude <= 1f)
            {
                return;
            }

            EnforceScrollerHidden();
            AnimateScrollOffsetTo(targetOffset);
        }

        /// <summary>
        ///     ノードへのズームインを解除し、ズーム前の拡大率とスクロール位置へ戻す。
        /// </summary>
        public void ClearFocusZoom()
        {
            if (_isDisposed || !_isZoomed)
            {
                return;
            }

            _isZoomed = false;
            _skillTreeRoot.RemoveFromClassList(ZOOMED_USS_CLASS);
            _skillTreeRoot.style.scale = StyleKeyword.Null;
            _skillTreeRoot.style.transformOrigin = StyleKeyword.Null;
            _currentScale = BASE_SCALE;
            EnforceScrollerHidden();
            AnimateScrollOffsetTo(new Vector2(_scrollOffsetXBeforeZoom, _scrollOffsetYBeforeZoom));
        }

        /// <summary>
        ///     指定ノード群の外接矩形が画面に収まるようズームし、中央に表示する。
        ///     連続解放演出などで、解放パスの最下ノードと最上ノードを画面の下端・上端に
        ///     揃えるために使用する。縦横のフィット倍率のうち小さい方を採用するため、
        ///     幅が広いパスでは横方向が優先され、縦方向には余白ができる。
        ///     FocusOnNodeまたはClearFocusZoomを呼ぶことで元の表示へ戻る。
        /// </summary>
        /// <param name="nodeIds"> フレーミング対象のノードID群。 </param>
        public void FocusOnNodeRange(IReadOnlyList<int> nodeIds)
        {
            if (_isDisposed || nodeIds == null || nodeIds.Count == 0)
            {
                return;
            }

            if (!TryGetNodesLocalBounds(nodeIds, out Rect localBounds))
            {
                return;
            }

            Rect viewportLayout = _scrollView.contentViewport.layout;
            if (!IsValidLength(viewportLayout.width) || !IsValidLength(viewportLayout.height)
                || !IsValidLength(localBounds.width) || !IsValidLength(localBounds.height))
            {
                return;
            }

            Vector2 localCenter = localBounds.center;
            Vector2 worldCenterBeforeChange = _skillTreeRoot.LocalToWorld(localCenter);

            // 基点切替(旧基点→パス中心)の瞬間、旧倍率が掛かったままパス中心自身の
            // 画面上位置もΔ=(新基点-旧基点)×(1-旧倍率)だけ瞬時にジャンプする。
            // 切替前に読んだ位置はこのジャンプ後を反映していないため、ここで予測して補正する。
            Vector2 predictedCenter = worldCenterBeforeChange;
            if (_isZoomed)
            {
                Vector2 delta = (localCenter - _focusedNodeLocalCenter) * (1f - _currentScale);
                predictedCenter += delta;
            }

            Rect viewportWorldBounds = _scrollView.contentViewport.worldBound;
            if (!IsValidRect(viewportWorldBounds))
            {
                return;
            }

            EnforceScrollerHidden();

            if (!_isZoomed)
            {
                _scrollOffsetXBeforeZoom = _scrollView.scrollOffset.x;
                _scrollOffsetYBeforeZoom = _scrollView.scrollOffset.y;
                _isZoomed = true;
            }

            _skillTreeRoot.style.transformOrigin = new TransformOrigin(localCenter.x, localCenter.y);
            _focusedNodeLocalCenter = localCenter;
            _skillTreeRoot.RemoveFromClassList(ZOOMED_USS_CLASS);

            // 外接矩形はノード中心基準のため、そのまま画面幅いっぱいに合わせると
            // 端のノード自身の半径分が画面外へはみ出す。RANGE_FOCUS_MARGIN分だけ
            // 内側に余白を確保してから倍率を計算する。
            float availableHeight = Mathf.Max(1f, viewportLayout.height - RANGE_FOCUS_MARGIN * 2f);
            float availableWidth = Mathf.Max(1f, viewportLayout.width - RANGE_FOCUS_MARGIN * 2f);
            float fitScaleY = availableHeight / localBounds.height;
            float fitScaleX = availableWidth / localBounds.width;
            // 下限は設けない。縮小を制限すると縦横どちらかの端がフレーム外へ切れてしまうため、
            // パス全体を収めるために必要な倍率までそのまま縮小させる。
            float rangeScale = Mathf.Min(fitScaleX, fitScaleY, ZOOM_SCALE);
            _skillTreeRoot.style.scale = new StyleScale(new Scale(new Vector2(rangeScale, rangeScale)));
            _currentScale = rangeScale;

            float targetCenterInContentY = predictedCenter.y
                - viewportWorldBounds.yMin
                + _scrollView.scrollOffset.y;
            float focusY = viewportWorldBounds.height * 0.5f;
            float targetCenterInContentX = predictedCenter.x
                - viewportWorldBounds.xMin
                + _scrollView.scrollOffset.x;
            float focusX = viewportWorldBounds.width * 0.5f;
            AnimateScrollOffsetTo(new Vector2(
                ClampScrollOffsetX(targetCenterInContentX - focusX),
                ClampScrollOffsetY(targetCenterInContentY - focusY)));
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

            _zoomScrollAnimationItem?.Pause();
            _skillTreeRoot.RemoveFromClassList(ZOOMED_USS_CLASS);
            CancelPendingLayout();
            _focusTargetNodeIds = Array.Empty<int>();
            _isFocusRequested = false;
            _isZoomed = false;
            _isDisposed = true;
        }

        private const string SKILL_TREE_SCREEN_ROOT_NAME = "SkillTreeScreenRoot";
        private const string SKILL_TREE_CONTAINER_NAME = "SkillTreeContainer";
        private const string SKILL_TREE_ROOT_NAME = "SkillTreeRoot";
        private const string POINTS_NAME = "Points";
        private const string ZOOMED_USS_CLASS = "skill-tree-canvas--zoomed";
        private const float FOCUS_POSITION_RATIO_X = 0.5f;
        private const float FOCUS_POSITION_RATIO_Y = 0.7f;
        private const float POINTS_SAFE_MARGIN = 24.0f;
        private const long LAYOUT_RETRY_DELAY_MILLISECONDS = 16L;
        private const float ZOOM_ANIMATION_DURATION_SECONDS = 0.35f;
        private const long ZOOM_ANIMATION_INTERVAL_MS = 16L;

        /// <summary>
        ///     ズーム時の拡大率。SkillNode.ussの.skill-tree-canvas--zoomedのscale値と一致させること。
        /// </summary>
        private const float ZOOM_SCALE = 1.8f;

        /// <summary>
        ///     未ズーム時の基準倍率。SkillNode.ussの.skill-tree-canvasのscale値と一致させること。
        /// </summary>
        private const float BASE_SCALE = 1.3f;

        /// <summary>
        ///     範囲フレーミング時、外接矩形(ノード中心基準)の周囲に確保する画面上の余白(ピクセル)。
        ///     ノード自身の半径分を吸収し、端のノードが画面外へはみ出さないようにする。
        /// </summary>
        private const float RANGE_FOCUS_MARGIN = 70f;

        /// <summary> <see cref="EnsureVisible"/>でノードの周囲に確保する画面端からの最小余白(ピクセル)。 </summary>
        private const float ENSURE_VISIBLE_MARGIN = 60f;

        private readonly VisualElement _screenRoot;
        private readonly ScrollView _scrollView;
        private readonly VisualElement _skillTreeRoot;
        private readonly VisualElement _points;
        private readonly Dictionary<int, VisualElement> _nodeElements;
        private IVisualElementScheduledItem _pendingLayoutItem;
        private IVisualElementScheduledItem _zoomScrollAnimationItem;
        private int[] _focusTargetNodeIds = Array.Empty<int>();
        private float _scrollOffsetYBeforeZoom;
        private float _scrollOffsetXBeforeZoom;
        private Vector2 _focusedNodeLocalCenter;
        private float _currentScale = BASE_SCALE;
        private bool _isZoomed;
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
            // フォーカスの要求が無い場合は何もしない。
            if (_isDisposed || !_isFocusRequested || _focusTargetNodeIds.Length == 0)
            {
                return;
            }

            VisualElement viewport = _scrollView.contentViewport;
            Rect viewportBounds = viewport.worldBound;
            Rect pointsBounds = _points.worldBound;
            // スクロールビューが見えていない場合は、要求だけ終える。
            if (!IsElementVisible(_scrollView))
            {
                CompleteFocusRequest();
                return;
            }

            // レイアウトがまだ決まっていない場合は、決まった後にやり直す。
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

            // 対象ノードを縦は既定の位置（ただしポイント表示に被らない位置）、横は既定の位置に来るようにスクロールする。
            float targetCenterInContentY = targetBounds.center.y
                - viewportBounds.yMin
                + _scrollView.scrollOffset.y;
            float defaultFocusY = viewportBounds.height * FOCUS_POSITION_RATIO_Y;
            float pointsTopInViewport = pointsBounds.yMin - viewportBounds.yMin;
            float pointsLimitedFocusY = pointsTopInViewport
                - targetBounds.height * 0.5f
                - POINTS_SAFE_MARGIN;
            float focusY = Mathf.Min(defaultFocusY, pointsLimitedFocusY);
            float scrollOffsetY = targetCenterInContentY - focusY;

            float targetCenterInContentX = targetBounds.center.x
                - viewportBounds.xMin
                + _scrollView.scrollOffset.x;
            float focusX = viewportBounds.width * FOCUS_POSITION_RATIO_X;
            float scrollOffsetX = targetCenterInContentX - focusX;

            CompleteFocusRequest();
            ApplyFocus(new Vector2(scrollOffsetX, scrollOffsetY));
        }

        /// <summary>
        ///     スクロール位置を現在のスクロール範囲へクランプして適用する。
        /// </summary>
        /// <param name="scrollOffsetTarget"> 適用するX/Y方向のスクロール量。 </param>
        private void ApplyFocus(Vector2 scrollOffsetTarget)
        {
            if (_isDisposed || !IsElementVisible(_scrollView))
            {
                return;
            }

            float lowValueY = _scrollView.verticalScroller.lowValue;
            float highValueY = _scrollView.verticalScroller.highValue;
            float lowValueX = _scrollView.horizontalScroller.lowValue;
            float highValueX = _scrollView.horizontalScroller.highValue;
            if (!IsFinite(lowValueY) || !IsFinite(highValueY)
                || !IsFinite(lowValueX) || !IsFinite(highValueX))
            {
                return;
            }

            _scrollView.scrollOffset = new Vector2(
                Mathf.Clamp(scrollOffsetTarget.x, lowValueX, highValueX),
                Mathf.Clamp(scrollOffsetTarget.y, lowValueY, highValueY));
        }

        /// <summary>
        ///     Y方向のスクロールオフセットを現在のスクロール範囲へクランプする。
        /// </summary>
        /// <param name="rawScrollOffsetY"> クランプ前のスクロールオフセットY。 </param>
        /// <returns> クランプ後のスクロールオフセットY。範囲が取得できない場合は入力値をそのまま返す。 </returns>
        private float ClampScrollOffsetY(float rawScrollOffsetY)
        {
            float lowValue = _scrollView.verticalScroller.lowValue;
            float highValue = _scrollView.verticalScroller.highValue;
            if (!IsFinite(lowValue) || !IsFinite(highValue))
            {
                return rawScrollOffsetY;
            }

            return Mathf.Clamp(rawScrollOffsetY, lowValue, highValue);
        }

        /// <summary>
        ///     X方向のスクロールオフセットを現在のスクロール範囲へクランプする。
        /// </summary>
        /// <param name="rawScrollOffsetX"> クランプ前のスクロールオフセットX。 </param>
        /// <returns> クランプ後のスクロールオフセットX。範囲が取得できない場合は入力値をそのまま返す。 </returns>
        private float ClampScrollOffsetX(float rawScrollOffsetX)
        {
            float lowValue = _scrollView.horizontalScroller.lowValue;
            float highValue = _scrollView.horizontalScroller.highValue;
            if (!IsFinite(lowValue) || !IsFinite(highValue))
            {
                return rawScrollOffsetX;
            }

            return Mathf.Clamp(rawScrollOffsetX, lowValue, highValue);
        }

        /// <summary>
        ///     ScrollViewのスクロール位置を指定値まで滑らかに移動させる。
        ///     scrollOffsetはUSSトランジション非対応のため、スケジューラで手動補間する。
        /// </summary>
        /// <param name="targetOffset"> 目標のスクロールオフセット。 </param>
        private void AnimateScrollOffsetTo(Vector2 targetOffset)
        {
            _zoomScrollAnimationItem?.Pause();

            Vector2 startOffset = _scrollView.scrollOffset;
            float elapsedSeconds = 0.0f;
            _zoomScrollAnimationItem = _scrollView.schedule.Execute(() =>
            {
                elapsedSeconds += ZOOM_ANIMATION_INTERVAL_MS / 1000.0f;
                float t = Mathf.Clamp01(elapsedSeconds / ZOOM_ANIMATION_DURATION_SECONDS);
                float eased = 1.0f - Mathf.Pow(1.0f - t, 3.0f);
                _scrollView.scrollOffset = Vector2.Lerp(startOffset, targetOffset, eased);
                EnforceScrollerHidden();
                if (t >= 1.0f) { _zoomScrollAnimationItem.Pause(); }
            }).Every(ZOOM_ANIMATION_INTERVAL_MS);
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
            // 対象ノードすべてを含む範囲を求める。レイアウトが済んでいないノードがあれば待つ。
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
        ///     指定ノード群の、SkillTreeRoot基準ローカル座標での外接矩形を取得する。
        /// </summary>
        /// <param name="nodeIds"> 対象ノードID群。 </param>
        /// <param name="localBounds"> 算出したローカル座標の外接矩形。 </param>
        /// <returns> 1件以上のノードが解決できた場合はtrue。 </returns>
        private bool TryGetNodesLocalBounds(IReadOnlyList<int> nodeIds, out Rect localBounds)
        {
            localBounds = default;
            bool hasBounds = false;
            for (int i = 0; i < nodeIds.Count; i++)
            {
                if (!_nodeElements.TryGetValue(nodeIds[i], out VisualElement nodeElement))
                {
                    continue;
                }

                Rect nodeWorldBound = nodeElement.worldBound;
                if (!IsValidRect(nodeWorldBound))
                {
                    continue;
                }

                Vector2 nodeLocalCenter = _skillTreeRoot.WorldToLocal(nodeWorldBound.center);
                if (!hasBounds)
                {
                    localBounds = new Rect(nodeLocalCenter, Vector2.zero);
                    hasBounds = true;
                    continue;
                }

                localBounds.xMin = Mathf.Min(localBounds.xMin, nodeLocalCenter.x);
                localBounds.xMax = Mathf.Max(localBounds.xMax, nodeLocalCenter.x);
                localBounds.yMin = Mathf.Min(localBounds.yMin, nodeLocalCenter.y);
                localBounds.yMax = Mathf.Max(localBounds.yMax, nodeLocalCenter.y);
            }

            return hasBounds;
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
