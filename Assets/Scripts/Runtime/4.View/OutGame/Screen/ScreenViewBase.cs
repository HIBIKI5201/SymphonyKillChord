using System;
using System.Threading;
using System.Threading.Tasks;
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
            CreateBrocker();
            RootElement = rootElement;
            OutGameUIEvent = outGameUIEvent;
            _brocker.RemoveFromHierarchy();
        }

        /// <summary>
        ///     画面を表示します。
        /// </summary>
        public virtual async Task Show(CancellationToken token)
        {
            // 画面表示中はブロッカーを配置して、ユーザーの操作を受け付けないようにする。
            RootElement.Add(_brocker);

            RootElement.style.display = DisplayStyle.Flex;
            RootElement.AddToClassList(VISIBLE_CLASS);
            RootElement.RemoveFromClassList(HIDDEN_CLASS);
            RootElement.BringToFront();
            _brocker.BringToFront();

            await WaitForTransitionEndAsync(token);

            _brocker.RemoveFromHierarchy();
        }

        /// <summary>
        ///     画面を非表示にします。
        /// </summary>
        public virtual async Task Hide(CancellationToken token)
        {
            // 画面非表示中はブロッカーを配置して、ユーザーの操作を受け付けないようにする。
            RootElement.Add(_brocker);

            RootElement.AddToClassList(HIDDEN_CLASS);
            RootElement.RemoveFromClassList(VISIBLE_CLASS);
            _brocker.BringToFront();

            await WaitForTransitionEndAsync(token);

            RootElement.style.display = DisplayStyle.None;
            _brocker.RemoveFromHierarchy();
        }

        /// <summary>
        ///     即座に画面を非表示にします。
        /// </summary>
        public virtual void HideImmediately()
        {
            RootElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public virtual void Dispose() { }

        /// <summary> USSの画面表示用クラス名。 </summary>
        protected const string VISIBLE_CLASS = "screen-visible";
        /// <summary> USSの画面非表示用クラス名。 </summary>
        protected const string HIDDEN_CLASS = "screen-hidden";
        /// <summary> トランジションのタイムアウト秒数。 </summary>
        protected const float TRANSITION_TIMEOUT_SEC = 1.0f;

        /// <summary> VisualElement のルート要素を取得します。 </summary>
        protected VisualElement RootElement { get; }
        /// <summary> OutGameUIEvent を取得します。 </summary>
        protected OutGameUIEvent OutGameUIEvent { get; }

        private VisualElement _brocker;

        /// <summary>
        ///     RootElement の TransitionEnd イベントを Task に変換して待機します。
        ///     Transition が設定されていない場合や、タイムアウト時は即座に完了します。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task WaitForTransitionEndAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnTransitionEnd(TransitionEndEvent _)
            {
                tcs.TrySetResult(true);
            }

            RootElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            // token がキャンセルされた時、 tcs を完了させる。
            // これにより、キャンセルされた場合も待機が終了する。
            using CancellationTokenRegistration registration = token.Register(() => tcs.TrySetResult(true));

            try
            {
                await Task.WhenAny(
                    tcs.Task,
                    // タイムアウトはキャンセルトークンの影響を受けないようにする。
                    Task.Delay(TimeSpan.FromSeconds(TRANSITION_TIMEOUT_SEC), CancellationToken.None));  
            }
            finally
            {
                RootElement.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
        }

        /// <summary>
        ///     ブロッカーを生成。
        /// </summary>
        /// <returns></returns>
        private void CreateBrocker()
        {
            // ブロッカーを生成して、画面全体を覆うように配置する。
            _brocker = new VisualElement();
            _brocker.style.position = Position.Absolute;
            _brocker.style.top = 0;
            _brocker.style.left = 0;
            _brocker.style.right = 0;
            _brocker.style.bottom = 0;

            // ブロッカーは透明にして、ユーザーの操作を受け付けないようにする。
            _brocker.style.backgroundColor = new UnityEngine.Color(0, 0, 0, 0.0f);

            // ブロッカーがユーザーの操作を受け付ける。
            _brocker.pickingMode = PickingMode.Position;
        }
    }
}
