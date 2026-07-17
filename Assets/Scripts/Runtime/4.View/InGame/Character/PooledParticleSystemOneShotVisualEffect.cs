using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     ParticleSystemプールを再生するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class PooledParticleSystemOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     再利用ParticleSystemViewからエフェクトを再生します。
        /// </summary>
        /// <param name="cueName"> 未使用のCueNameです。 </param>
        public void Play(string cueName)
        {
            _particleSystemPlaybackView?.Play();
        }

        /// <summary>
        ///     再利用ParticleSystemViewの再生中エフェクトを停止します。
        /// </summary>
        public void Stop()
        {
            _particleSystemPlaybackView?.StopAll();
        }

        [SerializeField, Tooltip("足音時に再生する再利用ParticleSystemViewです。PoolとRingBufferの両方を指定できます。")]
        private ReusableParticleSystemView _particleSystemPlaybackView;
    }
}
