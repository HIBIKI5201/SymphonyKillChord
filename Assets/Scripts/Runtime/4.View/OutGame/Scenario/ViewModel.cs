using System;
using KillChord.Runtime.Adaptor;
namespace KillChord.Runtime.View
{
    public class ViewModel : ITextViewSink, IFadeViewSink, IBackgroundViewSink, IAnimationViewSink
        , IScenarioCompletionViewSink
    {
        public void SetText(string message)
        {
            OnChat?.Invoke(message);
        }

        public void SetFade(float start, float end, float duration)
        {
            OnFade?.Invoke(start, end, duration);
        }

        public void SetBackground(string assetKey)
        {
            OnBackground?.Invoke(assetKey);
        }

        public void SetAnimation(string animationId)
        {
            OnAnimation?.Invoke(animationId);
        }

        public void SetScenarioCompleted(bool skipped)
        {
            OnScenarioCompleted?.Invoke(skipped);
        }

        public Action<string> OnChat;
        public Action<float, float, float> OnFade;
        public Action<string> OnBackground;
        public Action<string> OnAnimation;
        public Action<bool> OnScenarioCompleted;

    }
}
