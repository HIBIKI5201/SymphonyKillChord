using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     足音演出を再生するViewです。
    /// </summary>
    public sealed class FootStepView : MonoBehaviour
    {
        /// <summary>
        ///     足音演出を再生します。
        /// </summary>
        /// <param name="cueName"> 再生するCueNameです。 </param>
        public void Play(string cueName)
        {
            if (_oneShotVisualEffects == null)
            {
                return;
            }

            foreach (IOneShotVisualEffect oneShotVisualEffect in _oneShotVisualEffects)
            {
                oneShotVisualEffect?.Play(cueName);
            }
        }

        [SerializeReference, SubclassSelector, Tooltip("足音時に発火するワンショット演出です。")]
        private IOneShotVisualEffect[] _oneShotVisualEffects = Array.Empty<IOneShotVisualEffect>();

        /// <summary>
        ///     無効化時に演出の再生状態を初期化します。
        /// </summary>
        private void OnDisable()
        {
            if (_oneShotVisualEffects == null)
            {
                return;
            }

            foreach (IOneShotVisualEffect oneShotVisualEffect in _oneShotVisualEffects)
            {
                oneShotVisualEffect?.Stop();
            }
        }
    }
}
