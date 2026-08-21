using KillChord.Runtime.Adaptor.Persistent.Music;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Voice
{
    /// <summary>
    ///     登録されたVoice Sourceの音量を一括管理するクラス。
    /// </summary>
    public class VoiceVolumeManager : IVolumeManager
    {
        /// <summary>
        ///     Voice Sourceを登録します。
        /// </summary>
        /// <param name="source"> Voice Source。 </param>
        public void Register(VoiceSource source)
        {
            if (source == null || _sources.Contains(source))
            {
                return;
            }

            source.ApplyVolume(_volume);
            _sources.Add(source);
        }

        /// <summary>
        ///     Voice Sourceの登録を解除します。
        /// </summary>
        /// <param name="source"> Voice Source。 </param>
        public void UnRegister(VoiceSource source)
        {
            if (source == null)
            {
                return;
            }

            _sources.Remove(source);
        }

        /// <summary>
        ///     登録済みVoice Sourceへ音量を一括適用します。
        /// </summary>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);

            for (int i = _sources.Count - 1; i >= 0; i--)
            {
                if (_sources[i] == null)
                {
                    _sources.RemoveAt(i);
                    continue;
                }

                _sources[i].ApplyVolume(_volume);
            }
        }

        public float GetVolume()
        {
           return _volume;
        }

        private readonly List<VoiceSource> _sources = new();
        private float _volume = 0.5f;
    }
}
