using KillChord.Runtime.Adaptor.Persistent.Music;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     効果音の再生を管理するクラス。
    /// </summary>
    public class SoundEffectVolumeManager : IVolumeManager
    {
        /// <summary>
        ///     SE Sourceを登録する。
        /// </summary>
        /// <param name="source"> SESource。 </param>
        public void Register(SoundEffectSource source)
        {
            if (source == null || _sources.Contains(source))
            {
                return;
            }

            source.ApplyVolume(_volume);
            _sources.Add(source);
        }

        /// <summary>
        ///     SE Sourceの登録を解除する。
        /// </summary>
        /// <param name="source"> SESource。 </param>
        public void UnRegister(SoundEffectSource source)
        {
            if (source == null)
            {
                return;
            }

            _sources.Remove(source);
        }

        /// <summary>
        ///     全SESourceに音量を適用する。
        /// </summary>
        /// <param name="volume"> 音量。 </param>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);

            foreach (var source in _sources)
            {
                if (source == null)
                {
                    _sources.Remove(source);
                    continue;
                }

                source.ApplyVolume(_volume);
            }
        }

        private readonly List<SoundEffectSource> _sources = new();
        private float _volume = 1f;
    }
}