using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Audio
{
    /// <summary>
    ///     永続音量管理の登録窓口を提供するViewです。
    /// </summary>
    public sealed class PersistentAudioVolumeRegistryView : MonoBehaviour
    {
        /// <summary>
        ///     音量管理システムを初期化する。
        /// </summary>
        /// <param name="soundEffectVolumeManager"> SE音量管理です。 </param>
        /// <param name="voiceVolumeManager"> Voice音量管理です。 </param>
        public void Initialize(
            SoundEffectVolumeManager soundEffectVolumeManager,
            VoiceVolumeManager voiceVolumeManager)
        {
            _soundEffectVolumeManager = soundEffectVolumeManager
                ?? throw new ArgumentNullException(nameof(soundEffectVolumeManager));
            _voiceVolumeManager = voiceVolumeManager
                ?? throw new ArgumentNullException(nameof(voiceVolumeManager));
        }

        /// <summary>
        ///     SE Source を登録する。
        /// </summary>
        /// <param name="source"> 登録対象です。 </param>
        public void RegisterSoundEffectSource(SoundEffectSource source)
        {
            _soundEffectVolumeManager?.Register(source);
        }

        /// <summary>
        ///     SE Source の登録を解除する。
        /// </summary>
        /// <param name="source"> 解除対象です。 </param>
        public void UnregisterSoundEffectSource(SoundEffectSource source)
        {
            _soundEffectVolumeManager?.UnRegister(source);
        }

        /// <summary>
        ///     Voice Source を登録する。
        /// </summary>
        /// <param name="source"> 登録対象です。 </param>
        public void RegisterVoiceSource(VoiceSource source)
        {
            _voiceVolumeManager?.Register(source);
        }

        /// <summary>
        ///     Voice Source の登録を解除する。
        /// </summary>
        /// <param name="source"> 解除対象です。 </param>
        public void UnregisterVoiceSource(VoiceSource source)
        {
            _voiceVolumeManager?.UnRegister(source);
        }

        private SoundEffectVolumeManager _soundEffectVolumeManager;
        private VoiceVolumeManager _voiceVolumeManager;
    }
}
