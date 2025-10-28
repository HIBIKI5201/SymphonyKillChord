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
        private double _baseBeatLength; // 最小公分母から計算された基本拍の長さ
        private StringBuilder _debugLog = new StringBuilder(); // デバッグログ用

        private void Start()
        {
            // 最小公分母を計算して基本拍の長さを設定
            _baseBeatLength = CalculateLCDBeatLength();
            Debug.Log($"Base Beat Length (LCD): {_baseBeatLength:F3}");
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
            double quantizedBeat = QuantizeBeat(beat); // 基本拍の長さでクオンタイズ

            int mostNearTimingIndex = -1;
            double mostNearTimingValue = double.MaxValue;

            // 最後の入力からの拍数を計算し、最も近いタイミングを見つける。
            if (0 < _inputedTimingList.Count)
            {
                double lastBeat = _inputedTimingList.Last(); // 最後の入力の拍を取得。
                double betweenBeat = quantizedBeat - lastBeat;

                _debugLog.AppendLine($"Input Beat: {beat:F3}, Quantized Beat: {quantizedBeat:F3}, Last Beat: {lastBeat:F3}, Between: {betweenBeat:F3}");

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

                // 最も近い拍子を常に選択
                float detectedBeatLength = 4f / _timeSignatures[mostNearTimingIndex];

                _debugLog.AppendLine($"Detected Beat Length: {detectedBeatLength:F3} ({_timeSignatures[mostNearTimingIndex]}拍子), Timing Diff: {mostNearTimingValue:F3}");

                _musicUI.CreateNote(_noteColor[mostNearTimingIndex]);
                _inputedTimingList.Enqueue(quantizedBeat);
                _debugLog.AppendLine($"Note created with color index {mostNearTimingIndex} ({_timeSignatures[mostNearTimingIndex]}拍子)");
            }
            else
            {
                // 初回入力もクオンタイズされたタイミングを記録。
                _debugLog.AppendLine($"First Input - Beat: {beat:F3}, Quantized Beat: {quantizedBeat:F3}");
                _musicUI.CreateNote(_noteColor[0]); // 初回は最初の色を使用
                _inputedTimingList.Enqueue(quantizedBeat);
                
                // 初回入力のクオンタイズ情報も表示
                double multiplier = beat / _baseBeatLength;
                double roundedMultiplier = Math.Round(multiplier);
                _debugLog.AppendLine($"First Input Quantize: Multiplier={multiplier:F3}, Rounded={roundedMultiplier:F0}");
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

        /// <summary>
        /// 拍子の最小公分母から基本拍の長さを計算する
        /// </summary>
        private double CalculateLCDBeatLength()
        {
            // 各拍子の拍の長さを分数として表現
            List<(int numerator, int denominator)> fractions = new List<(int, int)>();
            
            foreach (float timeSignature in _timeSignatures)
            {
                // 4/timeSignature を分数に変換
                int numerator = 4;
                int denominator = (int)timeSignature;
                
                // 約分
                int gcd = CalculateGCD(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;
                
                fractions.Add((numerator, denominator));
            }

            // 最小公分母を計算
            int lcd = CalculateLCD(fractions.Select(f => f.denominator).ToList());
            
            // 基本拍の長さ = 1/lcd
            return 1.0 / lcd;
        }

        /// <summary>
        /// 複数の整数の最小公倍数を計算する
        /// </summary>
        private int CalculateLCM(List<int> values)
        {
            if (values.Count == 0) return 1;
            if (values.Count == 1) return values[0];

            int result = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                result = CalculateLCM(result, values[i]);
            }
            return result;
        }

        /// <summary>
        /// 2つの整数の最小公倍数を計算する
        /// </summary>
        private int CalculateLCM(int a, int b)
        {
            return (a * b) / CalculateGCD(a, b);
        }

        /// <summary>
        /// 複数の整数の最小公分母を計算する
        /// </summary>
        private int CalculateLCD(List<int> denominators)
        {
            return CalculateLCM(denominators);
        }

        /// <summary>
        /// 2つの整数の最大公約数を計算する
        /// </summary>
        private int CalculateGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// 指定された拍を基本拍の長さでクオンタイズする
        /// </summary>
        private double QuantizeBeat(double beat)
        {
            if (!_enableQuantize) return beat;

            // 基本拍の長さの倍数に最も近い値にクオンタイズ
            double multiplier = beat / _baseBeatLength;
            double roundedMultiplier = Math.Round(multiplier);
            double quantizedBeat = roundedMultiplier * _baseBeatLength;
            
            _debugLog.AppendLine($"Quantize: Beat={beat:F3}, Multiplier={multiplier:F3}, Rounded={roundedMultiplier:F0}, Quantized={quantizedBeat:F3}");
            
            return quantizedBeat;
        }
    }
}