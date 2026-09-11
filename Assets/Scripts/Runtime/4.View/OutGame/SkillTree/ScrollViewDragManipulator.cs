using System;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     ドラッグスクロールの対象方向。
    /// </summary>
    public enum ScrollDragAxis
    {
        /// <summary> 縦方向のみをドラッグでスクロールする。 </summary>
        Vertical,

        /// <summary> 横方向のみをドラッグでスクロールする。 </summary>
        Horizontal,
    }

    /// <summary>
    ///     ScrollView を、左クリックホールド中のマウス移動でドラッグスクロールできるようにするマニピュレータ。
    ///     ボタンなどのクリック可能な子要素上から操作を始めても、移動量が閾値を超えるまではクリックとして扱う。
    /// </summary>
    public sealed class ScrollViewDragManipulator : PointerManipulator
    {
        /// <summary>
        ///     マニピュレータを初期化し、対象の ScrollView へアタッチする。
        /// </summary>
        /// <param name="scrollView"> ドラッグスクロール対象の ScrollView。 </param>
        /// <param name="axis"> ドラッグスクロールの対象方向。既定は縦方向。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public ScrollViewDragManipulator(ScrollView scrollView, ScrollDragAxis axis = ScrollDragAxis.Vertical)
        {
            _scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            _axis = axis;
            target = scrollView;
        }

        /// <summary>
        ///     必要なポインタイベントのコールバックを対象へ登録する。
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            // ノード(Button)にポインタ操作を奪われないよう、トリクルダウン(先取り)で購読する。
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        /// <summary>
        ///     登録したポインタイベントのコールバックを対象から解除する。
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private const float DRAG_START_THRESHOLD = 10f;

        /// <summary>
        ///     ドラッグが確定した瞬間(移動量が閾値を超えた瞬間)に発火する。
        /// </summary>
        public event Action DragStarted;

        private readonly ScrollView _scrollView;
        private readonly ScrollDragAxis _axis;

        private bool _isPointerDown;
        private bool _isDragging;
        private int _activePointerId = -1;

        // ドラッグ開始時のポインタ座標を保持するフィールド。
        private Vector2 _pointerStartPanel;

        // ドラッグ開始時の scrollOffset を保持するフィールド。
        private Vector2 _scrollOffsetAtStart;

        /// <summary>
        ///     ポインタダウンを処理し、ドラッグ開始候補として座標と現在のスクロール位置を記録する。
        /// </summary>
        /// <param name="evt"> ポインタダウンイベント。 </param>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.pointerType == PointerType.mouse && evt.button != 0)
            {
                return;
            }

            _isPointerDown = true;
            _isDragging = false;
            _activePointerId = evt.pointerId;
            _pointerStartPanel = (Vector2)evt.position;
            _scrollOffsetAtStart = _scrollView.scrollOffset;
        }

        /// <summary>
        ///     ポインタ移動を処理する。移動量が閾値を超えた時点で初めてドラッグとして確定し、
        ///     以降はポインタの移動量に応じて scrollOffset を更新する。
        /// </summary>
        /// <param name="evt"> ポインタ移動イベント。 </param>
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isPointerDown || evt.pointerId != _activePointerId)
            {
                return;
            }

            if (_isDragging && !target.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            Vector2 pointerCurrent = (Vector2)evt.position;
            Vector2 delta = pointerCurrent - _pointerStartPanel;

            if (!_isDragging)
            {
                if (delta.sqrMagnitude < DRAG_START_THRESHOLD * DRAG_START_THRESHOLD)
                {
                    return;
                }

                _isDragging = true;
                DragStarted?.Invoke();

                // ここで初めてキャプチャすることで、閾値未満の操作はノード側のクリックとして残す。
                target.CapturePointer(evt.pointerId);
            }

            ApplyScroll(delta);
            evt.StopPropagation();
        }

        /// <summary>
        ///     ポインタアップを処理する。ドラッグが確定していた場合はクリックとして伝播させない。
        /// </summary>
        /// <param name="evt"> ポインタアップイベント。 </param>
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isPointerDown || evt.pointerId != _activePointerId)
            {
                return;
            }

            bool wasDragging = _isDragging;
            _isPointerDown = false;
            _isDragging = false;
            _activePointerId = -1;

            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }

            if (wasDragging)
            {
                evt.StopPropagation();
            }
        }

        /// <summary>
        ///     何らかの理由でキャプチャが失われた場合に状態をリセットする。
        /// </summary>
        /// <param name="evt"> ポインタキャプチャアウトイベント。 </param>
        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId != _activePointerId)
            {
                return;
            }

            _isPointerDown = false;
            _isDragging = false;
            _activePointerId = -1;
        }

        /// <summary>
        ///     ポインタの移動量に応じて scrollOffset を更新する。コンストラクタで指定した方向のみを対象とする。
        /// </summary>
        /// <param name="delta"> ドラッグ開始位置からの移動量。 </param>
        private void ApplyScroll(Vector2 delta)
        {
            if (_axis == ScrollDragAxis.Vertical)
            {
                // contentContainerのレイアウトサイズは実際のコンテンツ幅と一致しない場合があるため、
                // ScrollViewが内部で管理しているScrollerのlowValue/highValueから範囲を取得する。
                float lowValue = _scrollView.verticalScroller.lowValue;
                float highValue = _scrollView.verticalScroller.highValue;
                float newOffsetY = Mathf.Clamp(_scrollOffsetAtStart.y - delta.y, lowValue, highValue);
                _scrollView.scrollOffset = new Vector2(_scrollView.scrollOffset.x, newOffsetY);
            }
            else
            {
                float lowValue = _scrollView.horizontalScroller.lowValue;
                float highValue = _scrollView.horizontalScroller.highValue;
                float newOffsetX = Mathf.Clamp(_scrollOffsetAtStart.x - delta.x, lowValue, highValue);
                _scrollView.scrollOffset = new Vector2(newOffsetX, _scrollView.scrollOffset.y);
            }
        }
    }
}
