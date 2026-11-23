using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Utility.Diagnostics;
using KillChord.Runtime.View.Persistent.Audio;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFramework.Services;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     SoundEffectVolumeManager関連の初期化をする。
    /// </summary>
    [RequireComponent(typeof(PersistentAudioVolumeRegistryView))]
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("KillChord/Composition/Music/SoundEffectInitializer")]
    public sealed class SoundEffectInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SoundEffectInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 30;

        [SerializeField, Tooltip("デバッグ用に毎フレーム音量をログへ出すか。")]
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

            _volumeRegistryView = GetComponent<PersistentAudioVolumeRegistryView>();
            _soundEffectVolumeManager = new SoundEffectVolumeManager();
            _voiceVolumeManager = new VoiceVolumeManager();
            if (!_volumeRegistryView.Initialize(_soundEffectVolumeManager, _voiceVolumeManager))
            {
                return false;
            }

            Partitura.Register(_soundEffectVolumeManager, ServiceLifetimeEnum.Persistent);
            Partitura.Register(_voiceVolumeManager, ServiceLifetimeEnum.Persistent);
            _initialized = true;
            return true;
        }

        /// <summary>
        ///     デバッグが有効な場合、ボイスと SE の音量をログへ出す。
        /// </summary>
        private void Update()
        {
            if (_isDebug
                && _voiceVolumeManager != null
                && _soundEffectVolumeManager != null)
            {
                DevLog.Log(_voiceVolumeManager.GetVolume());
                DevLog.Log(_soundEffectVolumeManager.GetVolume());
            }
        }

        /// <summary>
        ///     登録済みの音量管理を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (Partitura.TryGet(out VoiceVolumeManager registeredVoiceVolumeManager)
                && ReferenceEquals(registeredVoiceVolumeManager, _voiceVolumeManager))
            {
                Partitura.Unregister<VoiceVolumeManager>();
            }

            if (Partitura.TryGet(out SoundEffectVolumeManager registeredSoundEffectVolumeManager)
                && ReferenceEquals(registeredSoundEffectVolumeManager, _soundEffectVolumeManager))
            {
                Partitura.Unregister<SoundEffectVolumeManager>();
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
