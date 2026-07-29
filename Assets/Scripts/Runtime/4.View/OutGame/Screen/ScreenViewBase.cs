using LitMotion;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     UI Toolkit 用画面 View の規定クラス。
    /// </summary>
    public abstract class ScreenViewBase : IScreenView, IDisposable
    {
        /// <summary>
        ///     画面 View を初期化します。
        /// </summary>
        public ScreenViewBase(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
        {
            RootElement = rootElement;
            OutGameUIEvent = outGameUIEvent;

            _brocker = CreateBrocker();
            _currentOpacity = RootElement.resolvedStyle.opacity;
        }

        /// <summary>
        ///    画面を表示状態にします。opacity のフェードは LitMotion で再生します。
        ///    フェード完了(または cancellationToken のキャンセル)まで待機できます。
        /// </summary>
        public virtual ValueTask Show(CancellationToken cancellationToken = default)
        {
            _opacityMotionHandle.TryComplete();

            RootElement.style.display = DisplayStyle.Flex;
            RootElement.BringToFront();

            // フェード中は入力を受け付けないようブロッカーを最前面に配置する。
            RootElement.Add(_brocker);
            _brocker.BringToFront();

            _opacityMotionHandle = LMotion.Create(_currentOpacity, 1f, FADE_DURATION)
                .WithEase(FADE_IN_EASE)
                .WithOnComplete(RemoveBrocker)
                .Bind(this, static (opacity, state) => state.SetOpacity(opacity));

            return _opacityMotionHandle.ToValueTask(cancellationToken);
        }

        /// <summary>
        ///     画面を非表示状態にします。opacity のフェードは LitMotion で再生します。
        ///     フェード完了(または cancellationToken のキャンセル)まで待機できます。
        /// </summary>
        /// <remarks>
        ///     フェード完了後に display を切ってレイアウトから外します。
        ///     フェード中に display を切ると同フレームで消えてしまうため、完了コールバックで行います。
        /// </remarks>
        public virtual ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _opacityMotionHandle.TryComplete();

            // フェード中は入力を受け付けないようブロッカーを最前面に配置する。
            RootElement.Add(_brocker);
            _brocker.BringToFront();

            _opacityMotionHandle = LMotion.Create(_currentOpacity, 0f, FADE_DURATION)
                .WithEase(FADE_OUT_EASE)
                .WithOnComplete(HideRootElement)
                .Bind(this, static (opacity, state) => state.SetOpacity(opacity));

            return _opacityMotionHandle.ToValueTask(cancellationToken);
        }

        /// <summary>
        ///     画面をフェードなしで即座に非表示状態にします。初期化時など、表示状態の保証が必要な場面で使用します。
        /// </summary>
        public virtual void HideImmediately()
        {
            _opacityMotionHandle.TryComplete();

            SetOpacity(0f);
            RootElement.style.display = DisplayStyle.None;
            RemoveBrocker();
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public virtual void Dispose()
        {
            _opacityMotionHandle.TryCancel();
            _brocker.RemoveFromHierarchy();
        }

        /// <summary>
        ///     フェードアウト完了後に呼び出され、要素をレイアウトから外し、ブロッカーを取り除きます。
        /// </summary>
        private void HideRootElement()
        {
            RootElement.style.display = DisplayStyle.None;
            RemoveBrocker();
        }

        /// <summary>
        ///     入力ブロッカーをレイアウトから取り除きます。
        /// </summary>
        private void RemoveBrocker()
        {
            _brocker.RemoveFromHierarchy();
        }

        /// <summary>
        ///     opacity を書き込み、直近の値として保持します。
        /// </summary>
        /// <remarks>
        ///     resolvedStyle.opacity は UI Toolkit のスタイル解決パスを経てから反映されるため、
        ///     同フレームで再度フェードを開始する際の始点として使うと古い値を読んでしまいます。
        ///     そのため実際に書き込んだ値をこのフィールドで自前管理します。
        /// </remarks>
        private void SetOpacity(float opacity)
        {
            RootElement.style.opacity = opacity;
            _currentOpacity = opacity;
        }

        /// <summary>
        ///     画面全体を覆う入力ブロッカーを生成します。
        ///     フェード中に背後への入力が抜けないようにするためのものです。
        /// </summary>
        private static VisualElement CreateBrocker()
        {
            var brocker = new VisualElement
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
            return brocker;
        }

        /// <summary> フェードにかかる時間(秒)。 </summary>
        private const float FADE_DURATION = 0.2f;
        /// <summary> フェードインのイージング。 </summary>
        private const Ease FADE_IN_EASE = Ease.OutCirc;
        /// <summary> フェードアウトのイージング。 </summary>
        private const Ease FADE_OUT_EASE = Ease.OutCirc;

        /// <summary> VisualElement のルート要素を取得します。 </summary>
        protected VisualElement RootElement { get; }
        /// <summary> OutGameUIEvent を取得します。 </summary>
        protected OutGameUIEvent OutGameUIEvent { get; }

        /// <summary> フェード中の入力を遮断するブロッカー要素。 </summary>
        private readonly VisualElement _brocker;

        private MotionHandle _opacityMotionHandle;
        /// <summary> 直近に書き込んだ opacity。フェード再開時の始点として使用します。 </summary>
        private float _currentOpacity;
    }
}
