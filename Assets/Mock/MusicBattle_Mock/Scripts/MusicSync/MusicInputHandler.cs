using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽の入力を扱うクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicInputHandler : MonoBehaviour
    {
        #region Publicメソッド
        /// <summary>
        /// 初期化を行う。
        /// </summary>
        /// <param name="timeSignatures">入力によってなりえる拍子</param>
        public void Init(float[] timeSignatures)
        {
            _timeSignatures = timeSignatures;
            _longestSignatureIndex = GetLongestSignatureIndex();
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
                quantizedBeat = CalcQuantizedBeat(lastBeat, detectedTimeSignatureIndex);

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
                quantizedBeat = CalcQuantizedBeat(0, detectedTimeSignatureIndex);

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

        [SerializeField]
        private CriMusicBuffer _musicBuffer;
        [SerializeField, ReadOnly]
        private float[] _timeSignatures; // 拍子リスト
        [SerializeField]
        private bool _enableQuantize = true; // クオンタイズ機能の有効/無効

        private Queue<double> _inputedTimingList = new();
        private int _longestSignatureIndex;
        private StringBuilder _debugLog = new StringBuilder(); // デバッグログ用

        #region ライフサイクル
        private void Start()
        {
            Debug.Log("Music Input initialized with beat-based quantization");
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

        /// <summary>
        /// 1拍が最も長い拍子のIndexを取得する
        /// </summary>
        /// <returns>拍子リストのIndex</returns>
        private int GetLongestSignatureIndex()
        {
            return _timeSignatures[0] > _timeSignatures[_timeSignatures.Length - 1] ?
                _timeSignatures.Length - 1 : 0;
        }

        /// <summary>
        ///     入力の標準拍タイミングを算出する
        ///     前回入力からの間隔が最も長い拍子のを超えている場合、補正計算を行い、
        ///     BGMの固有拍子に基づき、直近の標準拍タイミングを計算結果とする
        /// </summary>
        /// <param name="lastBeat">前回入力タイミング</param>
        /// <param name="signatureIndex">最も近い拍子のリストIndex/param>
        /// <returns>BGM再生開始から今回入力直近までの長さ</returns>
        private double CalcQuantizedBeat(double lastBeat, int signatureIndex)
        {
            // 前回入力からの間隔が最長拍未満の場合、前回入力 + 検出拍子長さとする
            double beatLength = GetBeatLength(signatureIndex);
            if (signatureIndex != _longestSignatureIndex) {
                return lastBeat + beatLength;
            }

            // 前回入力からの間隔が最長拍以上の場合、補正計算を行う
            double ret = lastBeat;
            bool loopEndFlg = false;
            while (!loopEndFlg)
            {
                // 前回入力タイミングから、最長拍子の拍長を1回ずつ、超えない所まで加算する
                if(ret + beatLength < _musicBuffer.CurrentBeat)
                {
                    ret += beatLength;
                }
                else
                {
                    // そしてBGM固有拍子の拍長を1拍分ずつ加算し、入力時間直前の標準拍タイミングを算出する
                    while (!loopEndFlg)
                    {
                        if(ret + 1d < _musicBuffer.CurrentBeat)
                        {
                            ret += 1d;
                        }
                        else
                        {
                            loopEndFlg = true;
                        }
                    }
                }
            }
            // 最後に、入力ずれを補正する。入力が次の拍に近い場合、結果を次の拍とする
            // 多分、補正のやり方にはまだ検討する必要がある
            StringBuilder logStr = new StringBuilder();
            logStr.AppendLine($"最後の入力ズレ補正。補正前の結果：{ret}、現在拍：{_musicBuffer.CurrentBeat}");

            ret += _musicBuffer.CurrentBeat - ret > 0.8d ? 1d : 0d;

            logStr.AppendLine($"補正後の結果：{ret}");
            Debug.Log(logStr.ToString());
            return ret;
        }
        #endregion
    }
}