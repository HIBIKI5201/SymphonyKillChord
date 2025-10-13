using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     音楽のバッファリングするクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicBuffer : MonoBehaviour
    {
        public float CurrentBpm => _currentBpm;

        public float BeatLength => 60f / _currentBpm;
        public float CurrentBeat => _beat;

        public void Play(AudioSource source, float bpm)
        {
            source.Play();

            _currentSource = source;
            _currentBpm = bpm;
        }

        [SerializeField, ReadOnly, Tooltip("再生中のソース")]
        private AudioSource _currentSource;
        [SerializeField, ReadOnly, Tooltip("現在のBPM")]
        private float _currentBpm;

        private float _beat;

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
            float beat = _currentSource.time / (60f / _currentBpm);
            _beat = beat;
        }
    }
}