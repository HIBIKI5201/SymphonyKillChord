using CriWare;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    public class CriMusicBuffer : MonoBehaviour, IMusicBuffer
    {
        public long CurrentBpm => _currentBpm;

        public long BeatLength => 60L / _currentBpm;
        public long CurrentBeat => _beat;

        public void Play(CriAtomSource source, long bpm)
        {
            source.Play();

            _currentSource = source;
            _currentBpm = bpm;
        }

        [SerializeField, ReadOnly, Tooltip("çƒê∂íÜÇÃÉ\Å[ÉX")]
        private CriAtomSource _currentSource;
        [SerializeField, ReadOnly, Tooltip("åªç›ÇÃBPM")]
        private long _currentBpm;

        private long _beat;

        private void Update()
        {
            Tick();
        }

        private void OnGUI()
        {
            if (_currentSource == null) return;

            GUILayout.Label($"Time: {_beat}");
        }

        private void Tick()
        {
            long beat = _currentSource.time / BeatLength;
            _beat = beat;
        }
    }
}