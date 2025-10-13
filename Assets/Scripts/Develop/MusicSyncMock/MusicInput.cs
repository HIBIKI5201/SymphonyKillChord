using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     音楽の入力を扱うクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicInput : MonoBehaviour
    {
        [SerializeField]
        private MusicBuffer _musicBuffer;

        [SerializeField]
        private float[] _timingBeats = { 1f, 2f, 3f, 4f };

        private List<float> _inputedTimingList = new();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space Key Down");
                HandleBeatInput();
            }
        }

        private void HandleBeatInput()
        {
            float beat = _musicBuffer.CurrentBeat; // 現在のビートを取得

            float mostNearTiming = 0;
            float mostNearTimingValue = float.MaxValue;

            // 最後の入力からのビート数を計算し、最も近いタイミングを見つける
            if (0 < _inputedTimingList.Count)
            {
                float lastBeat = _inputedTimingList[^1];
                float betweenBeat = beat - lastBeat;

                foreach (float timing in _timingBeats)
                {
                    float diff = Mathf.Abs(betweenBeat - timing);

                    if (diff < mostNearTimingValue)
                    {
                        mostNearTiming = timing;
                        mostNearTimingValue = diff;
                    }
                }

                Debug.Log($"Input Beat: {beat}, Last Beat: {lastBeat}, Between: {betweenBeat}, Nearest Timing Diff: {mostNearTiming}");
            }

            _inputedTimingList.Add(beat);

            if (_inputedTimingList.Count > 10) // 10以上は古い入力を削除。
            {
                _inputedTimingList.RemoveAt(0);
            }
        }
    }
}