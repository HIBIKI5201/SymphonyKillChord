using KillChord.Runtime.Adaptor.Persistent.Music;
using R3;
using System;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     UIへ公開する音量設定の表示状態を保持するViewModel。
    /// </summary>
    public sealed class AudioSettingsViewModel : IAudioSettingsViewModel, IDisposable
    {
        /// <summary>
        ///     音量設定ViewModelを初期化する。
        /// </summary>
        public AudioSettingsViewModel()
        {
            _bgmVolume = new ReactiveProperty<int>();
            _soundEffectVolume = new ReactiveProperty<int>();
            _voiceVolume = new ReactiveProperty<int>();
        }

        /// <summary> BGM音量。 </summary>
        public ReadOnlyReactiveProperty<int> BgmVolume => _bgmVolume;

        /// <summary> 効果音音量。 </summary>
        public ReadOnlyReactiveProperty<int> SoundEffectVolume => _soundEffectVolume;

        /// <summary> ボイス音量。 </summary>
        public ReadOnlyReactiveProperty<int> VoiceVolume => _voiceVolume;

        /// <summary>
        ///     表示用DTOを音量設定へ反映する。
        /// </summary>
        public void Apply(in AudioSettingsViewDTO dto)
        {
            _bgmVolume.Value = dto.BgmVolume;
            _soundEffectVolume.Value = dto.SoundEffectVolume;
            _voiceVolume.Value = dto.VoiceVolume;
        }

        /// <summary>
        ///     ReactivePropertyを解放する。
        /// </summary>
        public void Dispose()
        {
            _bgmVolume.Dispose();
            _soundEffectVolume.Dispose();
            _voiceVolume.Dispose();
        }

        private readonly ReactiveProperty<int> _bgmVolume;
        private readonly ReactiveProperty<int> _soundEffectVolume;
        private readonly ReactiveProperty<int> _voiceVolume;
    }
}
