using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        [SerializeField]
        private bool _enableQuantize = true; // クオンタイズ機能の有効/無効

        private Queue<double> _inputedTimingList = new();
        private StringBuilder _debugLog = new StringBuilder(); // デバッグログ用

        private void Start()
        {
            Debug.Log("Music Input initialized with beat-based quantization");
        }

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
            _debugLog.Clear(); // デバッグログをクリア
            _debugLog.AppendLine("=== Beat Input Debug ===");
            
            double beat = _musicBuffer.CurrentBeat; // 現在の拍を取得。

            int mostNearTimingIndex = -1;
            double mostNearTimingValue = double.MaxValue;
            double quantizedBeat = beat; // デフォルトは元の値

            // 最後の入力からの拍数を計算し、最も近いタイミングを見つける。
            if (0 < _inputedTimingList.Count)
            {
                double lastBeat = _inputedTimingList.Last(); // 最後の入力の拍を取得。
                double betweenBeat = beat - lastBeat;

                _debugLog.AppendLine($"Input Beat: {beat:F3}, Last Beat: {lastBeat:F3}, Between: {betweenBeat:F3}");

                for (int i = 0; i < _timeSignatures.Length; i++)
                {
                    float beatLength = 4f / _timeSignatures[i]; // 拍子の数から拍の長さを計算（4拍子基準）
                    double diff = Abs(betweenBeat - beatLength); // 最後の入力からの拍数とタイミングの差を計算。

                    _debugLog.AppendLine($"TimeSignature {i}: {_timeSignatures[i]}拍子, BeatLength: {beatLength:F3}, Diff: {diff:F3}");

                    // 最も近い拍を更新。
                    if (diff < mostNearTimingValue)
                    {
                        mostNearTimingIndex = i;
                        mostNearTimingValue = diff;
                    }
                }

                // 最も近い拍子のタイミングにクオンタイズ
                float detectedBeatLength = 4f / _timeSignatures[mostNearTimingIndex];
                quantizedBeat = lastBeat + detectedBeatLength;

                _debugLog.AppendLine($"Detected Beat Length: {detectedBeatLength:F3} ({_timeSignatures[mostNearTimingIndex]}拍子), Timing Diff: {mostNearTimingValue:F3}");
                _debugLog.AppendLine($"Quantized Beat: {quantizedBeat:F3}");

                _musicUI.CreateNote(_noteColor[mostNearTimingIndex]);
                _inputedTimingList.Enqueue(quantizedBeat);
                _debugLog.AppendLine($"Note created with color index {mostNearTimingIndex} ({_timeSignatures[mostNearTimingIndex]}拍子)");
            }
            else
            {
                // 初回入力は現在のタイミングをそのまま記録
                _debugLog.AppendLine($"First Input - Beat: {beat:F3}");
                _musicUI.CreateNote(_noteColor[0]); // 初回は最初の色を使用
                _inputedTimingList.Enqueue(beat);
            }

            if (_inputedTimingList.Count > 10) // 10個以上は古い入力を削除。
            {
                _inputedTimingList.Dequeue();
            }

            _debugLog.AppendLine($"Inputed Timing List: {string.Join(", ", _inputedTimingList.Select(b => b.ToString("F3")))}");
            
            // すべてのデバッグ情報を一括で出力
            Debug.Log(_debugLog.ToString());
        }

        private double Abs(double value) => value < 0 ? -value : value;

    }
}