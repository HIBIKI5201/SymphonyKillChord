using CriWare;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.View.Persistent.Audio;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     SE再生用のCRI Atom Sourceを管理システムへ登録するView。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource))]
    public class SoundEffectSource : MonoBehaviour, IPlayableAudioSource, IVolumeApplicable
    {
        /// <summary>
        ///     CriAtomSourceに設定されているCueを再生する。
        /// </summary>
        public void Play()
        {
            _source.Play();
        }

        /// <summary>
        ///     指定したCueNameを設定してSEを再生する。
        /// </summary>
        /// <param name="cueName"> SEのCue名。 </param>
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
        ///     音量を適用する。
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
            _volumeRegistryView?.RegisterSoundEffectSource(this);
        }

        private void OnDisable()
        {
            _volumeRegistryView?.UnregisterSoundEffectSource(this);
        }
    }
}
