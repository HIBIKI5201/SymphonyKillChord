using KillChord.Runtime.Adaptor.OutGame.Scenario;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// シナリオ表示用の通知を集約して View に渡す。
    /// </summary>
    public class ScenarioViewModel : ITextViewSink, IFadeViewSink, IBackgroundViewSink, IAnimationViewSink, IPortraitViewSink, ILayerViewSink
        , IScenarioCompletionViewSink
    {
        /// <summary>
        /// テキスト更新通知を購読先へ流す。
        /// </summary>
        public void SetText(in ScenarioTextViewDTO dto)
        {
            Speaker = dto.Speaker;
            Message = dto.Message;
            OnTextChanged?.Invoke();
        }

        /// <summary>
        /// 現在の話者名と本文を一括消去する。
        /// </summary>
        public void ClearText()
        {
            Speaker = string.Empty;
            Message = string.Empty;
            OnTextChanged?.Invoke();
        }

        /// <summary>
        /// 現在の話者名と本文を購読先へ再通知する。
        /// </summary>
        public void RefreshText()
        {
            OnTextChanged?.Invoke();
        }

        /// <summary>
        /// フェード更新を単一の View へ転送し、描画完了まで待機する。
        /// </summary>
        public ValueTask SetFadeAsync(in ScenarioFadeViewDTO dto, CancellationToken ct)
        {
            ScenarioFadeRequestHandler handler = _fadeRequestHandler;
            return handler != null ? handler(in dto, ct) : default;
        }

        /// <summary>
        /// フェード要求の処理先を設定する。
        /// </summary>
        public void BindFadeRequestHandler(ScenarioFadeRequestHandler handler)
        {
            _fadeRequestHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <summary>
        /// 指定したフェード要求処理先が現在の処理先なら解除する。
        /// </summary>
        public void UnbindFadeRequestHandler(ScenarioFadeRequestHandler handler)
        {
            if (_fadeRequestHandler == handler)
            {
                _fadeRequestHandler = null;
            }
        }

        /// <summary>
        /// 背景更新通知を購読先へ流す。
        /// </summary>
        public void SetBackground(string assetKey)
        {
            OnBackground?.Invoke(assetKey);
        }

        /// <summary>
        /// アニメーション更新通知を購読先へ流す。
        /// </summary>
        public void SetAnimation(string assetKey)
        {
            OnAnimation?.Invoke(assetKey);
        }

        /// <summary>
        /// 立ち絵更新通知を購読先へ流す。
        /// </summary>
        public void SetPortrait(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible)
        {
            OnPortrait?.Invoke(slot, assetKey, positionX, positionY, scale, visible);
        }

        /// <summary>
        /// レイヤー順更新通知を購読先へ流す。
        /// </summary>
        public void SetLayerOrder(string target, int order)
        {
            OnLayerOrder?.Invoke(target, order);
        }

        /// <summary>
        /// シナリオ完了通知を購読先へ流す。
        /// </summary>
        public void SetScenarioCompleted(bool skipped)
        {
            OnScenarioCompleted?.Invoke(skipped);
        }

        /// <summary> 現在の話者名を取得する。 </summary>
        public string Speaker { get; private set; } = string.Empty;
        /// <summary> 現在の本文を取得する。 </summary>
        public string Message { get; private set; } = string.Empty;
        /// <summary> 話者名または本文の変更通知を取得する。 </summary>
        public event Action OnTextChanged;
        /// <summary> OnBackground を取得する。 </summary>
        public event Action<string> OnBackground;
        /// <summary> OnAnimation を取得する。 </summary>
        public event Action<string> OnAnimation;
        /// <summary> OnPortrait を取得する。 </summary>
        public event Action<string, string, float, float, float, bool> OnPortrait;
        /// <summary> OnLayerOrder を取得する。 </summary>
        public event Action<string, int> OnLayerOrder;
        /// <summary> OnScenarioCompleted を取得する。 </summary>
        public event Action<bool> OnScenarioCompleted;

        private ScenarioFadeRequestHandler _fadeRequestHandler;
    }
}
