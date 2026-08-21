using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Music
{
    /// <summary>
    ///     UIから受け取った音量設定をDomainと各音量管理へ反映するController。
    /// </summary>
    public sealed class AudioSettingsController : IAudioSettingsCommand
    {
        /// <summary>
        ///     音量設定Controllerを初期化する。
        /// </summary>
        public AudioSettingsController(
            AudioSettingsData initialSettings,
            AudioSettingsService audioSettingsService,
            AudioSettingsPresenter audioSettingsPresenter,
            IVolumeManager bgmVolumeManager,
            IVolumeManager soundEffectVolumeManager,
            IVolumeManager voiceVolumeManager)
        {
            _settings = initialSettings?.Copy()
                ?? throw new ArgumentNullException(nameof(initialSettings));
            _audioSettingsService = audioSettingsService
                ?? throw new ArgumentNullException(nameof(audioSettingsService));
            _audioSettingsPresenter = audioSettingsPresenter
                ?? throw new ArgumentNullException(nameof(audioSettingsPresenter));
            _bgmVolumeManager = bgmVolumeManager
                ?? throw new ArgumentNullException(nameof(bgmVolumeManager));
            _soundEffectVolumeManager = soundEffectVolumeManager
                ?? throw new ArgumentNullException(nameof(soundEffectVolumeManager));
            _voiceVolumeManager = voiceVolumeManager
                ?? throw new ArgumentNullException(nameof(voiceVolumeManager));

            ApplyAllVolumes();
        }

        /// <summary>
        ///     BGM音量を設定する。
        /// </summary>
        public void SetBgmVolume(int volume)
        {
            int previousVolume = _settings.BgmVolume;
            _settings.SetBgmVolume(volume);
            if (previousVolume == _settings.BgmVolume)
            {
                return;
            }

            _bgmVolumeManager.SetVolume(ToInternalVolume(_settings.BgmVolume));
            SaveAndPresentSettings();
        }

        /// <summary>
        ///     効果音音量を設定する。
        /// </summary>
        public void SetSoundEffectVolume(int volume)
        {
            int previousVolume = _settings.SoundEffectVolume;
            _settings.SetSoundEffectVolume(volume);
            if (previousVolume == _settings.SoundEffectVolume)
            {
                return;
            }

            _soundEffectVolumeManager.SetVolume(ToInternalVolume(_settings.SoundEffectVolume));
            SaveAndPresentSettings();
        }

        /// <summary>
        ///     ボイス音量を設定する。
        /// </summary>
        public void SetVoiceVolume(int volume)
        {
            int previousVolume = _settings.VoiceVolume;
            _settings.SetVoiceVolume(volume);
            if (previousVolume == _settings.VoiceVolume)
            {
                return;
            }

            _voiceVolumeManager.SetVolume(ToInternalVolume(_settings.VoiceVolume));
            SaveAndPresentSettings();
        }

        /// <summary>
        ///     すべての音量を既定値へ戻す。
        /// </summary>
        public void ResetToDefaults()
        {
            _settings.SetVolumes(
                AudioSettingsData.DEFAULT_VOLUME,
                AudioSettingsData.DEFAULT_VOLUME,
                AudioSettingsData.DEFAULT_VOLUME);
            ApplyAllVolumes();
            SaveAndPresentSettings();
        }

        private const float INTERNAL_VOLUME_SCALE = 0.1f;

        private readonly AudioSettingsData _settings;
        private readonly AudioSettingsService _audioSettingsService;
        private readonly AudioSettingsPresenter _audioSettingsPresenter;
        private readonly IVolumeManager _bgmVolumeManager;
        private readonly IVolumeManager _soundEffectVolumeManager;
        private readonly IVolumeManager _voiceVolumeManager;

        /// <summary>
        ///     すべての音量管理へ現在値を適用する。
        /// </summary>
        private void ApplyAllVolumes()
        {
            _bgmVolumeManager.SetVolume(ToInternalVolume(_settings.BgmVolume));
            _soundEffectVolumeManager.SetVolume(ToInternalVolume(_settings.SoundEffectVolume));
            _voiceVolumeManager.SetVolume(ToInternalVolume(_settings.VoiceVolume));
        }

        /// <summary>
        ///     最新値の保存を要求し、表示状態へ反映する。
        /// </summary>
        private void SaveAndPresentSettings()
        {
            _audioSettingsService.QueueSave(_settings);
            _audioSettingsPresenter.Push(_settings);
        }

        /// <summary>
        ///     0～10の表示値を0～1の内部音量へ変換する。
        /// </summary>
        private static float ToInternalVolume(int volume)
        {
            return Mathf.Clamp(volume, AudioSettingsData.MIN_VOLUME, AudioSettingsData.MAX_VOLUME)
                * INTERNAL_VOLUME_SCALE;
        }
    }
}
