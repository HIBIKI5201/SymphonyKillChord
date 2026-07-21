using KillChord.Runtime.View.Persistent.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
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
}
