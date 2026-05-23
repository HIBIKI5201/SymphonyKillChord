using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノード間の接続線を表す View クラス。
    ///     解放時に塗りつぶしアニメーションを再生します。
    /// </summary>
    public sealed class StageNodeConnectionView : IStageConnectionViewModel
    {
        /// <summary>
        ///     StageNodeConnectionView を初期化します。
        /// </summary>
        /// <param name="root"> 接続線のルート VisualElement。</param>
        public StageNodeConnectionView(VisualElement root)
        {
            _root = root;

            _fill = root.Q<VisualElement>(FILL_ELEMENT_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(StageNodeConnectionView)}] {FILL_ELEMENT_NAME} が見つかりませんでした。");
        }

        /// <summary>
        ///     接続線の塗りつぶしアニメーションを再生します。
        ///     USS トランジションを使用して完了まで待機します。
        /// </summary>
        /// <param name="token"> キャンセルトークン。</param>
        public async Task PlayFillAnimationAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnTransitionEnd(TransitionEndEvent _)
            {
                tcs.TrySetResult(true);
            }

            // 接続線の向きに応じて、幅または高さを 100% に設定してアニメーションを開始。
            var isVertical = _root.resolvedStyle.height > _root.resolvedStyle.width;
            if (isVertical)
            {
                _fill.style.height = new Length(STYLESIZE_PERCENT, LengthUnit.Percent);
            }
            else
            {
                _fill.style.width = new Length(STYLESIZE_PERCENT, LengthUnit.Percent);
            }

            _fill.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            using var registration = token.Register(() => tcs.TrySetResult(true));

            try
            {
                await Task.WhenAny(
                    tcs.Task,
                    Task.Delay(TimeSpan.FromSeconds(ANIMATION_TIMEOUT_SEC), CancellationToken.None));
            }
            finally
            {
                _fill.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
        }

        /// <summary> Fill の幅または高さを 100% に設定するための定数。 </summary>
        private const int STYLESIZE_PERCENT = 100;
        /// <summary> アニメーションのタイムアウト秒数。 </summary>
        private const float ANIMATION_TIMEOUT_SEC = 5.0f;
        /// <summary> 塗りつぶし要素の名前。 </summary>
        private const string FILL_ELEMENT_NAME = "ConnectionFill";

        private readonly VisualElement _root;
        private readonly VisualElement _fill;
    }
}
