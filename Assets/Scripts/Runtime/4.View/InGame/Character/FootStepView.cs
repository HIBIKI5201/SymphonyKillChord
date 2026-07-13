using System;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using UnityEngine;
using UnityEngine.VFX;

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

    /// <summary>
    ///     ワンショット演出の再生契約です。
    /// </summary>
    public interface IOneShotVisualEffect
    {
        /// <summary>
        ///     ワンショット演出を再生します。
        /// </summary>
        /// <param name="cueName"> 足音SEに利用するCueNameです。 </param>
        void Play(string cueName);

        /// <summary>
        ///     ワンショット演出を停止します。
        /// </summary>
        void Stop();
    }

    /// <summary>
    ///     足音SEを再生するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class SoundEffectOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     足音SEを再生します。
        /// </summary>
        /// <param name="cueName"> 呼び出し元から渡されるCueNameです。 </param>
        public void Play(string cueName)
        {
            if (_soundEffectSource == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_cueNameOverride) && string.IsNullOrWhiteSpace(cueName))
            {
                _soundEffectSource.Play();
                return;
            }

            _soundEffectSource.Play(string.IsNullOrWhiteSpace(_cueNameOverride) ? cueName : _cueNameOverride);
        }

        /// <summary>
        ///     足音SEの停止処理は行いません。
        /// </summary>
        public void Stop()
        {
        }

        [SerializeField, Tooltip("足音SEのSourceです。")]
        private SoundEffectSource _soundEffectSource;

        [SerializeField, Tooltip("再生するCueNameを固定したい場合に指定します。空欄なら呼び出し時のCueNameを使います。")]
        private string _cueNameOverride;
    }

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
