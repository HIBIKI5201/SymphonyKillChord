using KillChord.Runtime.Adaptor.Persistent.Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Audio
{
    /// <summary>
    ///     登録された Source の音量を一括管理する共通実装クラス。
    /// </summary>
    /// <typeparam name="TSource"> 音量を適用する Source の型。 </typeparam>
    public abstract class AudioSourceVolumeManager<TSource> : IVolumeManager
        where TSource : MonoBehaviour, IVolumeApplicable
    {
        /// <summary>
        ///     音量設定の読み込み前に使う暫定音量を指定して初期化する。
        /// </summary>
        /// <param name="initialVolume"> 暫定音量。 </param>
        protected AudioSourceVolumeManager(float initialVolume)
        {
            _volume = Mathf.Clamp01(initialVolume);
        }

        /// <summary>
        ///     Source を登録し、現在の音量を適用する。
        /// </summary>
        /// <param name="source"> 登録する Source。 </param>
        public void Register(TSource source)
        {
            if (source == null || !_sources.Add(source))
            {
                return;
            }

            source.ApplyVolume(_volume);
        }

        /// <summary>
        ///     Source の登録を解除する。
        /// </summary>
        /// <param name="source"> 登録を解除する Source。 </param>
        public void UnRegister(TSource source)
        {
            if (source == null)
            {
                return;
            }

            _sources.Remove(source);
        }

        /// <summary>
        ///     登録済みの全 Source に音量を適用する。
        /// </summary>
        /// <param name="volume"> 音量。 </param>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);

            // 破棄済みの Source を除いてから、残りへ音量を適用する。
            _sources.RemoveWhere(IS_DESTROYED);
            foreach (TSource source in _sources)
            {
                source.ApplyVolume(_volume);
            }
        }

        /// <summary>
        ///     現在の音量を返す。
        /// </summary>
        /// <returns> 現在の音量。 </returns>
        public float GetVolume()
        {
            return _volume;
        }

        /// <summary> 破棄済みの Source かを判定する。呼び出しごとにデリゲートを生成しないよう保持する。 </summary>
        private static readonly Predicate<TSource> IS_DESTROYED = source => source == null;

        private readonly HashSet<TSource> _sources = new();
        /// <summary> 音量設定の読み込み前に登録されたSourceへ適用する暫定音量です。 </summary>
        private float _volume;
    }
}
