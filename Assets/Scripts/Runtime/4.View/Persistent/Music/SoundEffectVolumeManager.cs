using KillChord.Runtime.Adaptor.Persistent.Music;
using System;
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
            if (source == null || !_sources.Add(source))
            {
                return;
            }

            source.ApplyVolume(_volume);
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

            // 破棄済みの Source を除いてから、残りへ音量を適用する。
            _sources.RemoveWhere(IS_DESTROYED);
            foreach (SoundEffectSource source in _sources)
            {
                source.ApplyVolume(_volume);
            }
        }

        /// <summary>
        ///     現在の SE 音量を返す。
        /// </summary>
        public float GetVolume()
        {
            return _volume;
        }

        /// <summary> 破棄済みの Source かを判定する。呼び出しごとにデリゲートを生成しないよう保持する。 </summary>
        private static readonly Predicate<SoundEffectSource> IS_DESTROYED = source => source == null;

        private readonly HashSet<SoundEffectSource> _sources = new();
        /// <summary> 音量設定の読み込み前に登録されたSourceへ適用する暫定音量です。 </summary>
        private float _volume = 0.4f;
    }
}
