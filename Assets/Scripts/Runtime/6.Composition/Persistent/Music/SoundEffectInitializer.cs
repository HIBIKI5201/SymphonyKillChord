using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.View.Persistent.Audio;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     SoundEffectVolumeManager関連の初期化をする。
    /// </summary>
    [RequireComponent(typeof(PersistentAudioVolumeRegistryView))]
    [DefaultExecutionOrder(-1000)]
    public sealed class SoundEffectInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SoundEffectInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 30;

        [SerializeField]
        private bool _isDebug = true;
        private bool _initialized = false;
        private PersistentAudioVolumeRegistryView _volumeRegistryView;
        private SoundEffectVolumeManager _soundEffectVolumeManager;
        private VoiceVolumeManager _voiceVolumeManager;

        /// <summary>
        ///     音量管理システムを構築して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_initialized)
            {
                return true;
            }

            _initialized = true;
            _volumeRegistryView = GetComponent<PersistentAudioVolumeRegistryView>();
            _soundEffectVolumeManager = new SoundEffectVolumeManager();
            _voiceVolumeManager = new VoiceVolumeManager();
            _volumeRegistryView.Initialize(_soundEffectVolumeManager, _voiceVolumeManager);
            ServiceLocator.RegisterInstance(_soundEffectVolumeManager);
            ServiceLocator.RegisterInstance(_voiceVolumeManager);
            return true;
        }

        private void Update()
        {
            if (_isDebug
                && _voiceVolumeManager != null
                && _soundEffectVolumeManager != null)
            {
                Debug.Log(_voiceVolumeManager.GetVolume());
                Debug.Log(_soundEffectVolumeManager.GetVolume());
            }
        }

        /// <summary>
        ///     登録済みの音量管理を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out VoiceVolumeManager registeredVoiceVolumeManager)
                && ReferenceEquals(registeredVoiceVolumeManager, _voiceVolumeManager))
            {
                ServiceLocator.UnregisterInstance<VoiceVolumeManager>();
            }

            if (ServiceLocator.TryGetInstance(out SoundEffectVolumeManager registeredSoundEffectVolumeManager)
                && ReferenceEquals(registeredSoundEffectVolumeManager, _soundEffectVolumeManager))
            {
                ServiceLocator.UnregisterInstance<SoundEffectVolumeManager>();
            }

            _initialized = false;
            _volumeRegistryView = null;
            _soundEffectVolumeManager = null;
            _voiceVolumeManager = null;
        }

        /// <summary>
        ///     破棄時の安全側解除を行う。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
