using KillChord.Runtime.Domain.Persistent.Savedata;
using System;

namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     音量設定を表示用DTOへ変換してViewModelへ反映するPresenter。
    /// </summary>
    public sealed class AudioSettingsPresenter
    {
        /// <summary>
        ///     音量設定Presenterを初期化する。
        /// </summary>
        public AudioSettingsPresenter(IAudioSettingsViewModel audioSettingsViewModel)
        {
            _audioSettingsViewModel = audioSettingsViewModel
                ?? throw new ArgumentNullException(nameof(audioSettingsViewModel));
        }

        /// <summary>
        ///     現在の音量設定をViewModelへ反映する。
        /// </summary>
        public void Push(AudioSettingsData audioSettings)
        {
            if (audioSettings == null)
            {
                throw new ArgumentNullException(nameof(audioSettings));
            }

            AudioSettingsViewDTO dto = new AudioSettingsViewDTO(
                audioSettings.BgmVolume,
                audioSettings.SoundEffectVolume,
                audioSettings.VoiceVolume);
            _audioSettingsViewModel.Apply(in dto);
        }

        private readonly IAudioSettingsViewModel _audioSettingsViewModel;
    }
}
