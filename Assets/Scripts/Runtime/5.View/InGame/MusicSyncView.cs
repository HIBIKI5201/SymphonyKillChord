using CriWare;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        private CriAtomSource _cri;

        private CriAtomExPlayback _playback;

        public void Awake()
        {
            _cri = GetComponent<CriAtomSource>();
        }

        private void PlayBgm(string cueName)
        {
            _playback.Stop();
            _cri.cueName = cueName;
            _playback = _cri.Play();
        }

        private void Update()
        {
        }
    }
}