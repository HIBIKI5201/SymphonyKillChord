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
        private float[] _timeSignatures = { 4f, 8f, 3f, 6f }; // 拍子の数（4拍子、8拍子、3拍子、6拍子）
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
            double beat = _musicBuffer.CurrentBeat; // 現在の拍を取得。

            int mostNearTimingIndex = -1;
            double mostNearTimingValue = double.MaxValue;

            // 最後の入力からの拍数を計算し、最も近いタイミングを見つける。
            if (0 < _inputedTimingList.Count)
            {
                double lastBeat = _inputedTimingList.Last(); // 最後の入力の拍を取得。
                double betweenBeat = beat - lastBeat;

                for (int i = 0; i < _timeSignatures.Length; i++)
                {
                    float beatLength = 4f / _timeSignatures[i]; // 拍子の数から拍の長さを計算（4拍子基準）
                    double diff = Abs(betweenBeat - beatLength); // 最後の入力からの拍数とタイミングの差を計算。

                    // 最も近い拍を更新。
                    if (diff < mostNearTimingValue)
                    {
                        mostNearTimingIndex = i;
                        mostNearTimingValue = diff;
                    }
                }

                Debug.Log($"Input Beat: {beat}, Last Beat: {lastBeat}, Between: {betweenBeat}, Detected Beat Length: {4f / _timeSignatures[mostNearTimingIndex]} ({_timeSignatures[mostNearTimingIndex]}拍子)");

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