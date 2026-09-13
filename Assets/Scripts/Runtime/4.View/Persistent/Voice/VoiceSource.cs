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
        ///     Voice全体音量の比率を適用します。
        /// </summary>
        /// <param name="volumeRatio"> 0から1の音量比率。 </param>
        public void ApplyVolume(float volumeRatio)
        {
            _source.volume = _baseVolume * volumeRatio;
        }

        private CriAtomSource _source;
        private PersistentAudioVolumeRegistryView _volumeRegistryView;
        private float _baseVolume = 1f;
        private bool _baseVolumeCaptured;

        private void Awake()
        {
            _source = GetComponent<CriAtomSource>();
            CaptureBaseVolume();
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

        /// <summary>
        ///     CriAtomSourceに設定されている元の音量を保持します。
        /// </summary>
        private void CaptureBaseVolume()
        {
            if (_baseVolumeCaptured)
            {
                return;
            }

            _baseVolume = _source.volume;
            _baseVolumeCaptured = true;
        }
    }
}
