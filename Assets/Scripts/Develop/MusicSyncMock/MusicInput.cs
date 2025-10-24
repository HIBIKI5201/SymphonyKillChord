using System.Collections.Generic;
using System.Linq;
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
        private CriMusicBuffer _musicBuffer;
        [SerializeField]
        private MusicUI _musicUI;

        [SerializeField]
        private float[] _timingBeats = { 1f, 2f, 3f, 4f };
        [SerializeField]
        private Color[] _noteColor = { Color.white, Color.red, Color.blue, Color.green };

        private Queue<double> _inputedTimingList = new();

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
            double beat = _musicBuffer.CurrentBeat; // 現在のビートを取得

            int mostNearTimingIndex = -1;
            double mostNearTimingValue = float.MaxValue;

            // 最後の入力からのビート数を計算し、最も近いタイミングを見つける
            if (0 < _inputedTimingList.Count)
            {
                double lastBeat = _inputedTimingList.Last();
                double betweenBeat = beat - lastBeat;

                for (int i = 0; i < _timingBeats.Length; i++)
                {
                    float timing = _timingBeats[i];
                    double diff = Abs(betweenBeat - timing);

                    // 最も近いタイミングを更新。
                    if (diff < mostNearTimingValue)
                    {
                        mostNearTimingIndex = i;
                        mostNearTimingValue = diff;
                    }
                }

                Debug.Log($"Input Beat: {beat}, Last Beat: {lastBeat}, Between: {betweenBeat}, Nearest Timing Diff: {mostNearTimingIndex}");

                _musicUI.CreateNote(_noteColor[mostNearTimingIndex]);
            }

            _inputedTimingList.Enqueue(beat);

            if (_inputedTimingList.Count > 10) // 10以上は古い入力を削除。
            {
                _inputedTimingList.Dequeue();
            }
        }

        private double Abs(double value) => value < 0 ? -value : value;
    }
}