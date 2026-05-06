using CriWare;
using KillChord.Runtime.View.SoundEffect;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class SoundEffectPlayer : MonoBehaviour, IRecyclable
    {
        private CriAtomSource _source;
        private CriAtomExPlayback _playback;
        public int RecycleId { get; set; }

        public void OnRecycle()
        {
            _playback.Stop();
        }

        public void Play(string cueName)
        {
            _source.cueName = cueName;
            _playback = _source.Play();
        }
    }
}