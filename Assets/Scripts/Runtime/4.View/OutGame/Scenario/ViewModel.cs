using System;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// シナリオ表示用の通知を集約して View に渡す。
    /// </summary>
    public class ViewModel : ITextViewSink, IFadeViewSink, IBackgroundViewSink, IAnimationViewSink, IPortraitViewSink, ILayerViewSink
        , IScenarioCompletionViewSink
    {
        /// <summary>
        /// テキスト更新通知を購読先へ流す。
        /// </summary>
        public void SetText(string message)
        {
            OnChat?.Invoke(message);
        }

        /// <summary>
        /// フェード更新通知を購読先へ流す。
        /// </summary>
        public void SetFade(float start, float end, float duration)
        {
            OnFade?.Invoke(start, end, duration);
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

        /// <summary> OnChat を取得する。 </summary>
        public event Action<string> OnChat;
        /// <summary> OnFade を取得する。 </summary>
        public event Action<float, float, float> OnFade;
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

    }
}