using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     BGM、効果音、ボイスの音量設定を保持するセーブデータ。
    /// </summary>
    [Serializable]
    public sealed class AudioSettingsData
    {
        /// <summary>
        ///     音量設定を初期化する。
        /// </summary>
        /// <param name="bgmVolume"> BGM音量。 </param>
        /// <param name="soundEffectVolume"> 効果音音量。 </param>
        /// <param name="voiceVolume"> ボイス音量。 </param>
        public AudioSettingsData(
            int bgmVolume = DEFAULT_VOLUME,
            int soundEffectVolume = DEFAULT_VOLUME,
            int voiceVolume = DEFAULT_VOLUME)
        {
            SetVolumes(bgmVolume, soundEffectVolume, voiceVolume);
        }

        /// <summary> BGM音量。 </summary>
        public int BgmVolume => Mathf.Clamp(_bgmVolume, MIN_VOLUME, MAX_VOLUME);

        /// <summary> 効果音音量。 </summary>
        public int SoundEffectVolume => Mathf.Clamp(_soundEffectVolume, MIN_VOLUME, MAX_VOLUME);

        /// <summary> ボイス音量。 </summary>
        public int VoiceVolume => Mathf.Clamp(_voiceVolume, MIN_VOLUME, MAX_VOLUME);

        public const int MIN_VOLUME = 0;
        public const int MAX_VOLUME = 10;
        public const int DEFAULT_VOLUME = 5;

        /// <summary>
        ///     BGM音量を設定する。
        /// </summary>
        public void SetBgmVolume(int volume)
        {
            _bgmVolume = Clamp(volume);
        }

        /// <summary>
        ///     効果音音量を設定する。
        /// </summary>
        public void SetSoundEffectVolume(int volume)
        {
            _soundEffectVolume = Clamp(volume);
        }

        /// <summary>
        ///     ボイス音量を設定する。
        /// </summary>
        public void SetVoiceVolume(int volume)
        {
            _voiceVolume = Clamp(volume);
        }

        /// <summary>
        ///     3種類の音量をまとめて設定する。
        /// </summary>
        public void SetVolumes(int bgmVolume, int soundEffectVolume, int voiceVolume)
        {
            _bgmVolume = Clamp(bgmVolume);
            _soundEffectVolume = Clamp(soundEffectVolume);
            _voiceVolume = Clamp(voiceVolume);
        }

        /// <summary>
        ///     現在値の複製を作成する。
        /// </summary>
        public AudioSettingsData Copy()
        {
            return new AudioSettingsData(BgmVolume, SoundEffectVolume, VoiceVolume);
        }

        [SerializeField, Tooltip("BGM音量（0～10）")]
        private int _bgmVolume = DEFAULT_VOLUME;

        [SerializeField, Tooltip("効果音音量（0～10）")]
        private int _soundEffectVolume = DEFAULT_VOLUME;

        [SerializeField, Tooltip("ボイス音量（0～10）")]
        private int _voiceVolume = DEFAULT_VOLUME;

        /// <summary>
        ///     音量を有効範囲へ制限する。
        /// </summary>
        private static int Clamp(int Volume)
        {
            return Mathf.Clamp(Volume, MIN_VOLUME, MAX_VOLUME);
        }
    }
}
