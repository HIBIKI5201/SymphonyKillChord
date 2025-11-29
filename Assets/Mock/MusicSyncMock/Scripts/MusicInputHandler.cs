using SymphonyFrameWork.Attribute;
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
    public class MusicInputHandler : MonoBehaviour
    {
        [SerializeField]
        private CriMusicBuffer _musicBuffer;
        [SerializeField, ReadOnly]
        private float[] _timeSignatures; // 拍子リスト
        [SerializeField]
        private bool _enableQuantize = true; // クオンタイズ機能の有効/無効

        private Queue<double> _inputedTimingList = new();
        private StringBuilder _debugLog = new StringBuilder(); // デバッグログ用


        #region ライフサイクル
        private void Start()
        {
            Debug.Log("Music Input initialized with beat-based quantization");
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        /// 初期化を行う。
        /// </summary>
        /// <param name="timeSignatures">入力によってなりえる拍子</param>
        public void Init(float[] timeSignatures)
        {
            _timeSignatures = timeSignatures;
        }
        /// <summary>
        /// 入力されたタイミングを基いて、なりえる最も近い拍子を取得する。
        /// </summary>
        /// <param name="currentBeat">入力時点の全体拍数</param>
        /// <returns>最も近い拍子</returns>
        public float GetInputTimeSignature()
        {
            _debugLog.Clear(); // デバッグログをクリア
            _debugLog.AppendLine("=== Beat Input Debug ===");

            // 最も近い拍子の標準タイミング（拍数）
            double currentBeat = _musicBuffer.CurrentBeat;
            double quantizedBeat;
            int detectedTimeSignatureIndex;
            // 初回入力以外の場合
            if (0 < _inputedTimingList.Count)
            {
                // 最後の入力との差を計算する。
                double lastBeat = _inputedTimingList.Last();
                double betweenBeat = currentBeat - lastBeat;

                _debugLog.AppendLine($"Input Beat: {currentBeat:F3}, Last Beat: {lastBeat:F3}, Between: {betweenBeat:F3}");

                // 最も近い拍子を検出する。
                detectedTimeSignatureIndex = FindNearestTimeSignature(betweenBeat);
                // 最も近い拍子の拍の長さを取得して、最後の入力との合計を計算する。
                quantizedBeat = lastBeat + GetBeatLength(detectedTimeSignatureIndex);

                _debugLog.AppendLine($"Detected Beat Length: {GetBeatLength(detectedTimeSignatureIndex):F3} ({_timeSignatures[detectedTimeSignatureIndex]}拍子)");
                _debugLog.AppendLine($"Quantized Beat: {quantizedBeat:F3}");
            }
            else
            {
                // 初回入力処理
                _debugLog.AppendLine($"First Input - Beat: {currentBeat:F3}");

                // 最も近い拍子を検出する。
                detectedTimeSignatureIndex = FindNearestTimeSignature(currentBeat);
                // 最も近い拍子の拍の長さを取得して、最後の入力との合計を計算する。
                quantizedBeat = GetBeatLength(detectedTimeSignatureIndex);

                _debugLog.AppendLine($"First Input Detected Beat Length: {GetBeatLength(detectedTimeSignatureIndex):F3} ({_timeSignatures[detectedTimeSignatureIndex]}拍子)");
                _debugLog.AppendLine($"First Input Quantized Beat: {quantizedBeat:F3}");
            }
            _inputedTimingList.Enqueue(quantizedBeat);

            // 古い入力を削除
            if (_inputedTimingList.Count > 10)
            {
                _inputedTimingList.Dequeue();
            }

            _debugLog.AppendLine($"Inputed Timing List: {string.Join(", ", _inputedTimingList.Select(b => b.ToString("F3")))}");

            // すべてのデバッグ情報を一括で出力
            Debug.Log(_debugLog.ToString());
            return _timeSignatures[detectedTimeSignatureIndex];
        }

        #endregion

        #region Privateメソッド

        /// <summary>
        /// 指定された時間差に最も近い拍子のインデックスを取得する
        /// </summary>
        /// <param name="timeDifference">時間差</param>
        /// <returns>最も近い拍子のインデックス</returns>
        private int FindNearestTimeSignature(double timeDifference)
        {
            int mostNearTimingIndex = 0;
            double mostNearTimingValue = double.MaxValue;

            for (int i = 0; i < _timeSignatures.Length; i++)
            {
                double beatLength = GetBeatLength(i);
                double diff = Abs(timeDifference - beatLength);

                _debugLog.AppendLine($"TimeSignature {i}: {_timeSignatures[i]}拍子, BeatLength: {beatLength:F3}, Diff: {diff:F3}");

                if (diff < mostNearTimingValue)
                {
                    mostNearTimingIndex = i;
                    mostNearTimingValue = diff;
                }
            }

            return mostNearTimingIndex;
        }

        /// <summary>
        /// 指定された拍子インデックスの拍の長さを取得する
        /// </summary>
        /// <param name="timeSignatureIndex">拍子インデックス</param>
        /// <returns>拍の長さ</returns>
        private double GetBeatLength(int timeSignatureIndex) => _musicBuffer.PropTimeSignature / _timeSignatures[timeSignatureIndex];

        /// <summary>
        /// 指定された値の絶対値を取得する
        /// </summary>
        /// <param name="value">値</param>
        /// <returns>絶対値</returns>
        private double Abs(double value) => value < 0 ? -value : value;
        #endregion
    }
}