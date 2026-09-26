using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Persistent.Savedata;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     共通音量設定を読み込み、Persistentシーンへ公開するモジュール。
    /// </summary>
    public sealed class AudioSettingsInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(AudioSettingsInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 35;

        /// <summary>
        ///     保存済みの音量設定を読み込む。
        /// </summary>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _audioSettingsRepository = new AudioSettingsRepository();
            _audioSettingsService = new AudioSettingsService(_audioSettingsRepository);
            _loadedSettings = await _audioSettingsService.LoadAsync(cancellationToken);
            return _loadedSettings != null;
        }

        /// <summary>
        ///     音量管理へ保存値を適用し、音量設定Containerを登録する。
        /// </summary>
        public override bool Build()
        {
            if (!ServiceLocator.TryGetInstance(out MusicPlayer musicPlayer)
                || !ServiceLocator.TryGetInstance(out SoundEffectVolumeManager soundEffectVolumeManager)
                || !ServiceLocator.TryGetInstance(out VoiceVolumeManager voiceVolumeManager))
            {
                Debug.LogError(
                    $"[{nameof(AudioSettingsInitializer)}] 音量管理サービスを取得できませんでした。",
                    this);
                return false;
            }

            _audioSettingsViewModel = new AudioSettingsViewModel();
            _audioSettingsPresenter = new AudioSettingsPresenter(_audioSettingsViewModel);
            _audioSettingsController = new AudioSettingsController(
                _loadedSettings,
                _audioSettingsService,
                _audioSettingsPresenter,
                musicPlayer,
                soundEffectVolumeManager,
                voiceVolumeManager);
            _audioSettingsPresenter.Push(_loadedSettings);
            _moduleContainer = new AudioSettingsModuleContainer(
                _audioSettingsViewModel,
                _audioSettingsController);

            return ServiceLocator.RegisterInstance(_moduleContainer);
        }

        /// <summary>
        ///     音量設定Containerの登録を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out AudioSettingsModuleContainer registeredContainer)
                && ReferenceEquals(registeredContainer, _moduleContainer))
            {
                ServiceLocator.UnregisterInstance<AudioSettingsModuleContainer>();
            }

            _audioSettingsViewModel?.Dispose();
            _audioSettingsService?.Dispose();
            _moduleContainer = null;
            _audioSettingsController = null;
            _audioSettingsPresenter = null;
            _audioSettingsViewModel = null;
            _audioSettingsService = null;
            _audioSettingsRepository = null;
            _loadedSettings = null;
        }

        private IAudioSettingsRepository _audioSettingsRepository;
        private AudioSettingsService _audioSettingsService;
        private AudioSettingsViewModel _audioSettingsViewModel;
        private AudioSettingsPresenter _audioSettingsPresenter;
        private AudioSettingsController _audioSettingsController;
        private AudioSettingsModuleContainer _moduleContainer;
        private AudioSettingsData _loadedSettings;
    }
}
