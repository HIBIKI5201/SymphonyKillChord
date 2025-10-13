using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     音楽のバッファリングするクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicBuffer : MonoBehaviour, IMusicBuffer
    {
        public long CurrentBpm => _currentBpm;

        public long BeatLength => 60L / _currentBpm;
        public long CurrentBeat => _beat;

        public void Play(AudioSource source, long bpm)
        {
            source.Play();

            _currentSource = source;
            _currentBpm = bpm;
        }

        [SerializeField, ReadOnly, Tooltip("再生中のソース")]
        private AudioSource _currentSource;
        [SerializeField, ReadOnly, Tooltip("現在のBPM")]
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
            long beat = (long)(_currentSource.time / BeatLength);
            _beat = beat;
        }
    }
}