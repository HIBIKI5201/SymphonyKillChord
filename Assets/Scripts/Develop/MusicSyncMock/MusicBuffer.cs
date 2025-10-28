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
        public double CurrentBpm => _currentBpm;

        public double BeatLength => 60L / _currentBpm;
        public double CurrentBeat => _beat;

        public void Play(AudioSource source, double bpm)
        {
            source.Play();

            _currentSource = source;
            _currentBpm = bpm;
        }

        [SerializeField, ReadOnly, Tooltip("再生中のソース")]
        private AudioSource _currentSource;
        [SerializeField, ReadOnly, Tooltip("現在のBPM")]
        private double _currentBpm;

        private double _beat;

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
            double beat = _currentSource.time / BeatLength;
            _beat = beat;
        }
    }
}