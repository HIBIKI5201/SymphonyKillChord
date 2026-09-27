using KillChord.Runtime.Adaptor.Persistent.Music;
using System;
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
            if (source == null || !_sources.Add(source))
            {
                return;
            }

            source.ApplyVolume(_volume);
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

            // 破棄済みの Source を除いてから、残りへ音量を適用する。
            _sources.RemoveWhere(IS_DESTROYED);
            foreach (VoiceSource source in _sources)
            {
                source.ApplyVolume(_volume);
            }
        }

        /// <summary>
        ///     現在のボイス音量を返す。
        /// </summary>
        public float GetVolume()
        {
           return _volume;
        }

        /// <summary> 破棄済みの Source かを判定する。呼び出しごとにデリゲートを生成しないよう保持する。 </summary>
        private static readonly Predicate<VoiceSource> IS_DESTROYED = source => source == null;

        private readonly HashSet<VoiceSource> _sources = new();
        /// <summary> 音量設定の読み込み前に登録されたSourceへ適用する暫定音量です。 </summary>
        private float _volume = 0.8f;
    }
}
