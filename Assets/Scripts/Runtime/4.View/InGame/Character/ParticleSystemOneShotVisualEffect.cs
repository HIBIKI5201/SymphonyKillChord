using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     ParticleSystemを再生するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class ParticleSystemOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     ParticleSystemを再生します。
        /// </summary>
        /// <param name="cueName"> 未使用のCueNameです。 </param>
        public void Play(string cueName)
        {
            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Play();
        }

        /// <summary>
        ///     ParticleSystemを停止します。
        /// </summary>
        public void Stop()
        {
            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        [SerializeField, Tooltip("足音時に再生するParticleSystemです。")]
        private ParticleSystem _particleSystem;
    }
}
