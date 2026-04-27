using System;
using KillChord.Runtime.Adaptor;
namespace KillChord.Runtime.View
{
    public class ViewModel : ITextViewSink, IFadeViewSink, IBackgroundViewSink, IAnimationViewSink, IPortraitViewSink
        , IScenarioCompletionViewSink
    {
        public const string DefaultPortraitSlotId = "Default";

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

        public void SetPortrait(string slotId, string assetKey)
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                return;
            }

            string normalizedSlot = string.IsNullOrWhiteSpace(slotId) ? DefaultPortraitSlotId : slotId;
            OnPortrait?.Invoke(normalizedSlot, assetKey);
        }

        public void SetScenarioCompleted(bool skipped)
        {
            OnScenarioCompleted?.Invoke(skipped);
        }

        public event Action<string> OnChat;
        public event Action<float, float, float> OnFade;
        public event Action<string> OnBackground;
        public event Action<string> OnAnimation;
        public event Action<string, string> OnPortrait;
        public event Action<bool> OnScenarioCompleted;

    }
}
