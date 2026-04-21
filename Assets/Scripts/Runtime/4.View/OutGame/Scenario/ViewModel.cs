using System;
using KillChord.Runtime.Adaptor;
using UnityEngine;
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

        public void SetBackground(Sprite background)
        {
            OnBackground?.Invoke(background);
        }

        public void SetAnimation(AnimationClip animationClip)
        {
            OnAnimation?.Invoke(animationClip);
        }

        public void SetScenarioCompleted(bool skipped)
        {
            OnScenarioCompleted?.Invoke(skipped);
        }

        public Action<string> OnChat;
        public Action<float, float, float> OnFade;
        public Action<Sprite> OnBackground;
        public Action<AnimationClip> OnAnimation;
        public Action<bool> OnScenarioCompleted;

    }
}
