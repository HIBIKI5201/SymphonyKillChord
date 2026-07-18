using System;
using UnityEngine;
using UnityEngine.VFX;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     Visual Effect Graphを再生するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class VisualEffectGraphOneShotVisualEffect : IOneShotVisualEffect
    {
        private const string DEFAULT_EVENT_NAME = "OnPlay";

        /// <summary>
        ///     Visual Effect Graphを再生します。
        /// </summary>
        /// <param name="cueName"> 未使用のCueNameです。 </param>
        public void Play(string cueName)
        {
            if (_visualEffect == null)
            {
                return;
            }

            if (_reinitializeOnPlay)
            {
                _visualEffect.Reinit();
            }

            if (string.IsNullOrWhiteSpace(_eventName))
            {
                _visualEffect.Play();
                return;
            }

            _visualEffect.SendEvent(_eventName);
        }

        /// <summary>
        ///     Visual Effect Graphを停止します。
        /// </summary>
        public void Stop()
        {
            if (_visualEffect == null)
            {
                return;
            }

            _visualEffect.Stop();
        }

        [SerializeField, Tooltip("足音時に再生するVisual Effectです。")]
        private VisualEffect _visualEffect;

        [SerializeField, Tooltip("再生時に送るイベント名です。空欄ならPlayを呼びます。")]
        private string _eventName = DEFAULT_EVENT_NAME;

        [SerializeField, Tooltip("再生前にVisual EffectをReinitするかです。")]
        private bool _reinitializeOnPlay = true;
    }
}
