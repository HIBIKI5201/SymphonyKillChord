using CriWare;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.View.Persistent.Audio;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Voice
{
    /// <summary>
    ///     Voice再生用のCRI Atom SourceをVoice音量管理へ登録するView。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource))]
    public sealed class VoiceSource : MonoBehaviour, IPlayableAudioSource, IVolumeApplicable
    {
        /// <summary>
        ///     CriAtomSourceに設定されているVoice Cueを再生する。
        /// </summary>
        public void Play()
        {
            _source.Play();
        }

        /// <summary>
        ///     指定したCueNameを設定してVoiceを再生ｓ。 
        /// </summary>
        /// <param name="cueName"> VoiceのCue名。 </param>
        public void Play(string cueName)
        {
            if (string.IsNullOrWhiteSpace(cueName))
            {
                Play();
                return;
            }

            _source.cueName = cueName;
            _source.Play();
        }

        /// <summary>
        ///     Voiceを停止します。
        /// </summary>
        public void Stop()
        {
            _source.Stop();
        }

        /// <summary>
        ///     Voice音量を適用します。
        /// </summary>
        /// <param name="volume"> 音量。 </param> 
        public void ApplyVolume(float volume)
        {
            _source.volume = volume;
        }

        private CriAtomSource _source;
        private PersistentAudioVolumeRegistryView _volumeRegistryView;

        private void Awake()
        {
            _source = GetComponent<CriAtomSource>();
        }

        private void OnEnable()
        {
            _volumeRegistryView ??= FindAnyObjectByType<PersistentAudioVolumeRegistryView>();
            _volumeRegistryView?.RegisterVoiceSource(this);
        }

        private void OnDisable()
        {
            _volumeRegistryView?.UnregisterVoiceSource(this);
        }
    }
}
