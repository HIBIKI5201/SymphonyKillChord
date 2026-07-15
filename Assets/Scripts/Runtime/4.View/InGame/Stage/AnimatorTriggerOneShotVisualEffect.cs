using KillChord.Runtime.View.InGame.Character;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Stage
{
    /// <summary>
    ///     AnimatorのTriggerを発火するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class AnimatorTriggerOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     AnimatorのTriggerを発火します。
        /// </summary>
        /// <param name="cueName"> 使用しません。 </param>
        public void Play(string cueName)
        {
            if (_animator == null || string.IsNullOrWhiteSpace(_triggerName))
            {
                return;
            }

            _animator.ResetTrigger(_triggerName);
            _animator.SetTrigger(_triggerName);
        }

        /// <summary>
        ///     Triggerをリセットします。
        /// </summary>
        public void Stop()
        {
            if (_animator != null && !string.IsNullOrWhiteSpace(_triggerName))
            {
                _animator.ResetTrigger(_triggerName);
            }
        }

        [SerializeField, Tooltip("Triggerを発火するAnimatorです。")]
        private Animator _animator;

        [SerializeField, Tooltip("発火するTrigger名です。")]
        private string _triggerName;
    }
}
