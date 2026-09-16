using KillChord.Runtime.View.OutGame.Navigation;
using LitMotion;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Tutorial
{
    /// <summary>
    ///     ホーム画面の操作対象を順番に案内するチュートリアルオーバーレイです。
    /// </summary>
    public sealed class TutorialOverlayView : IDisposable
    {
        /// <summary>
        ///     チュートリアルオーバーレイを初期化します。
        /// </summary>
        /// <param name="parentElement"> オーバーレイを配置する親要素です。 </param>
        public TutorialOverlayView(VisualElement parentElement)
        {
            _parentElement = parentElement ?? throw new ArgumentNullException(nameof(parentElement));
            _modalNavigationScope = new ModalNavigationScope();
            _topCurtain = CreateCurtain();
            _bottomCurtain = CreateCurtain();
            _leftCurtain = CreateCurtain();
            _rightCurtain = CreateCurtain();
            _arrowBar = CreateArrowBar();
            _arrowTip = CreateArrowTip();
            _messageLabel = CreateMessageLabel();
            _messageBox = CreateMessageBox(_messageLabel);
            _root = CreateRoot();

            _root.Add(_topCurtain);
            _root.Add(_bottomCurtain);
            _root.Add(_leftCurtain);
            _root.Add(_rightCurtain);
            _root.Add(_arrowBar);
            _root.Add(_arrowTip);
            _root.Add(_messageBox);
            _root.MakeNavigable();
            _root.RegisterCallback<ClickEvent>(HandleClickHandler);
            _root.RegisterCallback<NavigationSubmitEvent>(HandleNavigationSubmitHandler);
        }

        /// <summary>
        ///     指定要素をハイライトし、説明文を表示します。
        ///     決定入力を受けるまで待機するValueTaskを返します。
        /// </summary>
        /// <param name="targetElement"> ハイライトする要素です。 </param>
        /// <param name="message"> 表示する説明文です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 決定入力を受けるまで待機するValueTaskです。 </returns>
        public ValueTask ShowStepAsync(
            VisualElement targetElement,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TutorialOverlayView));
            }

            if (targetElement == null)
            {
                throw new ArgumentNullException(nameof(targetElement));
            }

            _stepCancellationRegistration.Dispose();
            _stepCompletionSource?.TrySetCanceled();
            _stepCompletionSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource<bool> completionSource = _stepCompletionSource;
            _stepCancellationRegistration = cancellationToken.Register(
                () => completionSource.TrySetCanceled(cancellationToken));

            ApplyGuideStyles();
            _messageLabel.text = message;
            int layoutGeneration = ++_layoutGeneration;
            CancelLayoutWait();

            if (!_isActive)
            {
                _isActive = true;
                SetOpacity(0f);
                _parentElement.Add(_root);
                _root.BringToFront();
                _modalNavigationScope.Activate(_root);
                _root.FocusDeferred();

                _opacityMotionHandle = LMotion.Create(0f, 1f, FADE_DURATION)
                    .WithEase(FADE_EASE)
                    .Bind(this, static (opacity, state) => state.SetOpacity(opacity));

                WaitForLayoutAndUpdate(targetElement, layoutGeneration);
            }
            else
            {
                _root.BringToFront();

                // 表示済みのルートはレイアウト確定済みのため、従来どおり次の更新で配置する。
                _root.schedule.Execute(() => UpdateLayout(targetElement, layoutGeneration));
            }

            return new ValueTask(completionSource.Task);
        }

        /// <summary>
        ///     オーバーレイをフェードアウトし、フォーカス制御を解除してパネルから除去します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> フェードアウトの完了を待機するValueTaskです。 </returns>
        public ValueTask HideAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed || !_isActive)
            {
                return default;
            }

            _stepCancellationRegistration.Dispose();
            CancelLayoutWait();
            _opacityMotionHandle.TryComplete();
            _opacityMotionHandle = LMotion.Create(_currentOpacity, 0f, FADE_DURATION)
                .WithEase(FADE_EASE)
                .WithOnComplete(HandleHideCompletedHandler)
                .Bind(this, static (opacity, state) => state.SetOpacity(opacity));
            return _opacityMotionHandle.ToValueTask(cancellationToken);
        }

        /// <summary>
        ///     進行中の待機を中断し、要素を即座に除去します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _layoutGeneration++;
            _stepCancellationRegistration.Dispose();
            _stepCompletionSource?.TrySetCanceled();
            _opacityMotionHandle.TryCancel();
            CancelLayoutWait();
            _root.UnregisterCallback<ClickEvent>(HandleClickHandler);
            _root.UnregisterCallback<NavigationSubmitEvent>(HandleNavigationSubmitHandler);

            if (_modalNavigationScope.IsActive)
            {
                _modalNavigationScope.Deactivate();
            }

            _root.RemoveFromHierarchy();
            _isActive = false;
        }

        /// <summary> フェードにかかる時間(秒)です。 </summary>
        private const float FADE_DURATION = 0.2f;
        /// <summary> フェードのイージングです。 </summary>
        private const Ease FADE_EASE = Ease.OutCirc;
        /// <summary> 暗幕の不透明度です。 </summary>
        private const float CURTAIN_ALPHA = 0.75f;
        /// <summary> 説明パネルと案内矢印の表示倍率です。 </summary>
        private const float GUIDE_SCALE = 2.25f;
        /// <summary> 矢印の棒の幅です。 </summary>
        private const float ARROW_BAR_WIDTH = 6f * GUIDE_SCALE;
        /// <summary> 矢印の棒の長さです。 </summary>
        private const float ARROW_BAR_LENGTH = 64f * GUIDE_SCALE;
        /// <summary> 矢印の先端の直径です。 </summary>
        private const float ARROW_TIP_DIAMETER = 24f * GUIDE_SCALE;
        /// <summary> 説明文のフォントサイズです。 </summary>
        private const float MESSAGE_FONT_SIZE = 36f;
        /// <summary> 説明パネルの最大幅です。 </summary>
        private const float MESSAGE_BOX_MAX_WIDTH = 320f * GUIDE_SCALE;
        /// <summary> 説明パネルの内側余白です。 </summary>
        private const float MESSAGE_BOX_PADDING = 16f * GUIDE_SCALE;
        /// <summary> 説明パネルの角丸半径です。 </summary>
        private const float MESSAGE_BOX_RADIUS = 12f * GUIDE_SCALE;
        /// <summary> 説明パネルと矢印の間隔です。 </summary>
        private const float MESSAGE_BOX_GAP = 8f * GUIDE_SCALE;
        /// <summary> 画面端と説明パネルの間隔です。 </summary>
        private const float SCREEN_MARGIN = 16f;
        /// <summary> 説明パネルの背景色の明度です。 </summary>
        private const float MESSAGE_BOX_BACKGROUND_BRIGHTNESS = 0.05f;
        /// <summary> 説明パネルの背景色の不透明度です。 </summary>
        private const float MESSAGE_BOX_BACKGROUND_ALPHA = 0.9f;

        /// <summary> オーバーレイを配置する親要素です。 </summary>
        private readonly VisualElement _parentElement;
        /// <summary> オーバーレイ内にフォーカスを閉じ込めるスコープです。 </summary>
        private readonly ModalNavigationScope _modalNavigationScope;
        /// <summary> オーバーレイ全体を覆うルート要素です。 </summary>
        private readonly VisualElement _root;
        /// <summary> ハイライト対象より上側を覆う暗幕です。 </summary>
        private readonly VisualElement _topCurtain;
        /// <summary> ハイライト対象より下側を覆う暗幕です。 </summary>
        private readonly VisualElement _bottomCurtain;
        /// <summary> ハイライト対象より左側を覆う暗幕です。 </summary>
        private readonly VisualElement _leftCurtain;
        /// <summary> ハイライト対象より右側を覆う暗幕です。 </summary>
        private readonly VisualElement _rightCurtain;
        /// <summary> ハイライト対象から伸ばす矢印の棒です。 </summary>
        private readonly VisualElement _arrowBar;
        /// <summary> 矢印の先端に配置する丸です。 </summary>
        private readonly VisualElement _arrowTip;
        /// <summary> 説明文を表示するパネルです。 </summary>
        private readonly VisualElement _messageBox;
        /// <summary> チュートリアルの説明文を表示するラベルです。 </summary>
        private readonly Label _messageLabel;
        /// <summary> 現在の決定入力を待機する完了ソースです。 </summary>
        private TaskCompletionSource<bool> _stepCompletionSource;
        /// <summary> 現在の決定入力待機に対するキャンセル登録です。 </summary>
        private CancellationTokenRegistration _stepCancellationRegistration;
        /// <summary> 実行中の不透明度モーションです。 </summary>
        private MotionHandle _opacityMotionHandle;
        /// <summary> 直近に設定した不透明度です。 </summary>
        private float _currentOpacity;
        /// <summary> 遅延配置要求を識別する世代番号です。 </summary>
        private int _layoutGeneration;
        /// <summary> レイアウト確定を待機しているハイライト対象です。 </summary>
        private VisualElement _layoutTargetElement;
        /// <summary> レイアウト確定を待機している配置要求の世代番号です。 </summary>
        private int _waitingLayoutGeneration;
        /// <summary> レイアウト確定イベントを待機中の場合はtrueです。 </summary>
        private bool _isWaitingForLayout;
        /// <summary> オーバーレイがパネル上で有効な場合はtrueです。 </summary>
        private bool _isActive;
        /// <summary> リソースを解放済みの場合はtrueです。 </summary>
        private bool _isDisposed;

        /// <summary>
        ///     マウスクリックを決定入力として処理します。
        /// </summary>
        /// <param name="clickEvent"> クリックイベントです。 </param>
        private void HandleClickHandler(ClickEvent clickEvent)
        {
            clickEvent.StopPropagation();
            CompleteCurrentStep();
        }

        /// <summary>
        ///     ナビゲーション決定を決定入力として処理します。
        /// </summary>
        /// <param name="submitEvent"> ナビゲーション決定イベントです。 </param>
        private void HandleNavigationSubmitHandler(NavigationSubmitEvent submitEvent)
        {
            submitEvent.StopPropagation();
            CompleteCurrentStep();
        }

        /// <summary>
        ///     フェードアウト完了後にフォーカス制御を解除し、要素を除去します。
        /// </summary>
        private void HandleHideCompletedHandler()
        {
            CancelLayoutWait();

            if (_modalNavigationScope.IsActive)
            {
                _modalNavigationScope.Deactivate();
            }

            _root.RemoveFromHierarchy();
            _isActive = false;
        }

        /// <summary>
        ///     オーバーレイまたはハイライト対象のレイアウト変更後に配置を試みます。
        /// </summary>
        /// <param name="geometryChangedEvent"> レイアウト変更イベントです。 </param>
        private void HandleLayoutGeometryChangedHandler(
            GeometryChangedEvent geometryChangedEvent)
        {
            TryUpdateLayoutAfterGeometryChanged();
        }

        /// <summary>
        ///     現在のステップを決定入力があった場合と同様に完了させます。
        ///     タイマー満了などでチュートリアルを強制的に終了させたい場合に使用します。
        /// </summary>
        public void ForceCompleteCurrentStep()
        {
            CompleteCurrentStep();
        }

        /// <summary>
        ///     現在のステップの待機を完了します。
        /// </summary>
        private void CompleteCurrentStep()
        {
            _stepCompletionSource?.TrySetResult(true);
        }

        /// <summary>
        ///     オーバーレイとハイライト対象のレイアウト確定を待って配置します。
        /// </summary>
        /// <param name="targetElement"> ハイライトする要素です。 </param>
        /// <param name="layoutGeneration"> 配置要求の世代番号です。 </param>
        private void WaitForLayoutAndUpdate(
            VisualElement targetElement,
            int layoutGeneration)
        {
            _layoutTargetElement = targetElement;
            _waitingLayoutGeneration = layoutGeneration;
            _root.RegisterCallback<GeometryChangedEvent>(
                HandleLayoutGeometryChangedHandler);
            targetElement.RegisterCallback<GeometryChangedEvent>(
                HandleLayoutGeometryChangedHandler);
            _isWaitingForLayout = true;

            TryUpdateLayoutAfterGeometryChanged();
        }

        /// <summary>
        ///     レイアウト値が有効になった場合に待機を終了して配置します。
        /// </summary>
        private void TryUpdateLayoutAfterGeometryChanged()
        {
            if (!_isWaitingForLayout)
            {
                return;
            }

            VisualElement targetElement = _layoutTargetElement;
            int layoutGeneration = _waitingLayoutGeneration;
            if (_isDisposed || !_isActive || layoutGeneration != _layoutGeneration
                || _root.panel == null || targetElement?.panel == null)
            {
                CancelLayoutWait();
                return;
            }

            if (!IsLayoutReady(targetElement))
            {
                return;
            }

            CancelLayoutWait();
            UpdateLayout(targetElement, layoutGeneration);
        }

        /// <summary>
        ///     レイアウト確定イベントの待機を解除します。
        /// </summary>
        private void CancelLayoutWait()
        {
            if (!_isWaitingForLayout)
            {
                return;
            }

            _root.UnregisterCallback<GeometryChangedEvent>(
                HandleLayoutGeometryChangedHandler);
            _layoutTargetElement?.UnregisterCallback<GeometryChangedEvent>(
                HandleLayoutGeometryChangedHandler);
            _layoutTargetElement = null;
            _isWaitingForLayout = false;
        }

        /// <summary>
        ///     オーバーレイとハイライト対象の配置計算に必要な大きさが有効か判定します。
        /// </summary>
        /// <param name="targetElement"> ハイライトする要素です。 </param>
        /// <returns> すべての大きさが有限の正数の場合はtrueです。 </returns>
        private bool IsLayoutReady(VisualElement targetElement)
        {
            return IsValidLayoutLength(_root.resolvedStyle.width)
                && IsValidLayoutLength(_root.resolvedStyle.height)
                && IsValidLayoutLength(targetElement.worldBound.width)
                && IsValidLayoutLength(targetElement.worldBound.height);
        }

        /// <summary>
        ///     UIレイアウトから取得した長さが配置計算に使用可能か判定します。
        /// </summary>
        /// <param name="length"> 判定する長さです。 </param>
        /// <returns> 有限の正数の場合はtrueです。 </returns>
        private static bool IsValidLayoutLength(float length)
        {
            return !float.IsNaN(length)
                && !float.IsInfinity(length)
                && length > 0f;
        }

        /// <summary>
        ///     表示のたびに文字とパネルの実寸を設定し、生成済みの要素にも変更を反映します。
        /// </summary>
        private void ApplyGuideStyles()
        {
            _messageLabel.style.fontSize = MESSAGE_FONT_SIZE;
            // 以前の表示倍率が残っていても、レイアウトの実寸と描画寸法を一致させます。
            _messageBox.style.scale = new Scale(Vector2.one);
            _messageBox.style.maxWidth = MESSAGE_BOX_MAX_WIDTH;
            _messageBox.style.paddingTop = MESSAGE_BOX_PADDING;
            _messageBox.style.paddingRight = MESSAGE_BOX_PADDING;
            _messageBox.style.paddingBottom = MESSAGE_BOX_PADDING;
            _messageBox.style.paddingLeft = MESSAGE_BOX_PADDING;
            _messageBox.style.borderTopLeftRadius = MESSAGE_BOX_RADIUS;
            _messageBox.style.borderTopRightRadius = MESSAGE_BOX_RADIUS;
            _messageBox.style.borderBottomLeftRadius = MESSAGE_BOX_RADIUS;
            _messageBox.style.borderBottomRightRadius = MESSAGE_BOX_RADIUS;

            float arrowRadius = ARROW_TIP_DIAMETER * 0.5f;
            _arrowTip.style.borderTopLeftRadius = arrowRadius;
            _arrowTip.style.borderTopRightRadius = arrowRadius;
            _arrowTip.style.borderBottomLeftRadius = arrowRadius;
            _arrowTip.style.borderBottomRightRadius = arrowRadius;
        }

        /// <summary>
        ///     対象要素を基準に暗幕、矢印、説明パネルを配置します。
        /// </summary>
        /// <param name="targetElement"> ハイライトする要素です。 </param>
        /// <param name="layoutGeneration"> 配置要求の世代番号です。 </param>
        private void UpdateLayout(VisualElement targetElement, int layoutGeneration)
        {
            if (_isDisposed || !_isActive || layoutGeneration != _layoutGeneration
                || targetElement.panel == null)
            {
                return;
            }

            float overlayWidth = _root.resolvedStyle.width;
            float overlayHeight = _root.resolvedStyle.height;
            Vector2 targetMin = _root.WorldToLocal(targetElement.worldBound.min);
            Vector2 targetMax = _root.WorldToLocal(targetElement.worldBound.max);
            float targetLeft = Mathf.Clamp(targetMin.x, 0f, overlayWidth);
            float targetTop = Mathf.Clamp(targetMin.y, 0f, overlayHeight);
            float targetRight = Mathf.Clamp(targetMax.x, targetLeft, overlayWidth);
            float targetBottom = Mathf.Clamp(targetMax.y, targetTop, overlayHeight);

            SetRect(_topCurtain, 0f, 0f, overlayWidth, targetTop);
            SetRect(_bottomCurtain, 0f, targetBottom, overlayWidth, overlayHeight - targetBottom);
            SetRect(_leftCurtain, 0f, targetTop, targetLeft, targetBottom - targetTop);
            SetRect(
                _rightCurtain,
                targetRight,
                targetTop,
                overlayWidth - targetRight,
                targetBottom - targetTop);

            float targetCenterX = (targetLeft + targetRight) * 0.5f;
            bool isArrowDownward = (targetTop + targetBottom) * 0.5f < overlayHeight * 0.5f;
            float arrowBarTop = isArrowDownward
                ? targetBottom
                : targetTop - ARROW_BAR_LENGTH;
            float arrowTipTop = isArrowDownward
                ? arrowBarTop + ARROW_BAR_LENGTH - ARROW_TIP_DIAMETER * 0.5f
                : arrowBarTop - ARROW_TIP_DIAMETER * 0.5f;

            SetRect(
                _arrowBar,
                targetCenterX - ARROW_BAR_WIDTH * 0.5f,
                arrowBarTop,
                ARROW_BAR_WIDTH,
                ARROW_BAR_LENGTH);
            SetRect(
                _arrowTip,
                targetCenterX - ARROW_TIP_DIAMETER * 0.5f,
                arrowTipTop,
                ARROW_TIP_DIAMETER,
                ARROW_TIP_DIAMETER);

            _messageBox.style.width = Mathf.Min(
                MESSAGE_BOX_MAX_WIDTH,
                Mathf.Max(0f, overlayWidth - SCREEN_MARGIN * 2f));
            _messageBox.schedule.Execute(() => UpdateMessageBoxPosition(
                targetCenterX,
                arrowTipTop,
                isArrowDownward,
                layoutGeneration));
        }

        /// <summary>
        ///     レイアウト確定後の大きさを使って説明パネルを画面内に配置します。
        /// </summary>
        /// <param name="targetCenterX"> 対象要素の中心X座標です。 </param>
        /// <param name="arrowTipTop"> 矢印先端の上端座標です。 </param>
        /// <param name="isArrowDownward"> 矢印が下向きの場合はtrueです。 </param>
        /// <param name="layoutGeneration"> 配置要求の世代番号です。 </param>
        private void UpdateMessageBoxPosition(
            float targetCenterX,
            float arrowTipTop,
            bool isArrowDownward,
            int layoutGeneration)
        {
            if (_isDisposed || !_isActive || layoutGeneration != _layoutGeneration)
            {
                return;
            }

            float overlayWidth = _root.resolvedStyle.width;
            float overlayHeight = _root.resolvedStyle.height;
            float messageWidth = _messageBox.resolvedStyle.width;
            float messageHeight = _messageBox.resolvedStyle.height;
            float messageLeft = Mathf.Clamp(
                targetCenterX - messageWidth * 0.5f,
                SCREEN_MARGIN,
                Mathf.Max(SCREEN_MARGIN, overlayWidth - messageWidth - SCREEN_MARGIN));
            float desiredTop = isArrowDownward
                ? arrowTipTop + ARROW_TIP_DIAMETER + MESSAGE_BOX_GAP
                : arrowTipTop - MESSAGE_BOX_GAP - messageHeight;
            float messageTop = Mathf.Clamp(
                desiredTop,
                SCREEN_MARGIN,
                Mathf.Max(SCREEN_MARGIN, overlayHeight - messageHeight - SCREEN_MARGIN));

            _messageBox.style.left = messageLeft;
            _messageBox.style.top = messageTop;
        }

        /// <summary>
        ///     指定要素の絶対配置矩形を設定します。
        /// </summary>
        /// <param name="element"> 配置する要素です。 </param>
        /// <param name="left"> 左端座標です。 </param>
        /// <param name="top"> 上端座標です。 </param>
        /// <param name="width"> 幅です。 </param>
        /// <param name="height"> 高さです。 </param>
        private static void SetRect(
            VisualElement element,
            float left,
            float top,
            float width,
            float height)
        {
            element.style.left = left;
            element.style.top = top;
            element.style.width = Mathf.Max(0f, width);
            element.style.height = Mathf.Max(0f, height);
        }

        /// <summary>
        ///     オーバーレイ全体の不透明度を設定します。
        /// </summary>
        /// <param name="opacity"> 不透明度です。 </param>
        private void SetOpacity(float opacity)
        {
            _root.style.opacity = opacity;
            _currentOpacity = opacity;
        }

        /// <summary>
        ///     入力を遮断するルート要素を生成します。
        /// </summary>
        /// <returns> 生成したルート要素です。 </returns>
        private static VisualElement CreateRoot()
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    left = 0,
                    right = 0,
                    bottom = 0,
                    backgroundColor = new Color(0f, 0f, 0f, 0f),
                },
                pickingMode = PickingMode.Position,
            };
        }

        /// <summary>
        ///     ハイライト対象の周囲を覆う暗幕を生成します。
        /// </summary>
        /// <returns> 生成した暗幕です。 </returns>
        private static VisualElement CreateCurtain()
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    backgroundColor = new Color(0f, 0f, 0f, CURTAIN_ALPHA),
                },
                pickingMode = PickingMode.Ignore,
            };
        }

        /// <summary>
        ///     矢印の棒を生成します。
        /// </summary>
        /// <returns> 生成した矢印の棒です。 </returns>
        private static VisualElement CreateArrowBar()
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    backgroundColor = Color.white,
                },
                pickingMode = PickingMode.Ignore,
            };
        }

        /// <summary>
        ///     矢印の丸い先端を生成します。
        /// </summary>
        /// <returns> 生成した矢印の先端です。 </returns>
        private static VisualElement CreateArrowTip()
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    backgroundColor = Color.white,
                },
                pickingMode = PickingMode.Ignore,
            };
        }

        /// <summary>
        ///     説明文を表示するラベルを生成します。
        /// </summary>
        /// <returns> 生成したラベルです。 </returns>
        private static Label CreateMessageLabel()
        {
            return new Label
            {
                style =
                {
                    color = Color.white,
                    whiteSpace = WhiteSpace.Normal,
                },
                pickingMode = PickingMode.Ignore,
            };
        }

        /// <summary>
        ///     説明文を囲むパネルを生成します。
        /// </summary>
        /// <param name="messageLabel"> パネルに配置する説明ラベルです。 </param>
        /// <returns> 生成した説明パネルです。 </returns>
        private static VisualElement CreateMessageBox(Label messageLabel)
        {
            var messageBox = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    backgroundColor = new Color(
                        MESSAGE_BOX_BACKGROUND_BRIGHTNESS,
                        MESSAGE_BOX_BACKGROUND_BRIGHTNESS,
                        MESSAGE_BOX_BACKGROUND_BRIGHTNESS,
                        MESSAGE_BOX_BACKGROUND_ALPHA),
                },
                pickingMode = PickingMode.Ignore,
            };
            messageBox.Add(messageLabel);
            return messageBox;
        }
    }
}
