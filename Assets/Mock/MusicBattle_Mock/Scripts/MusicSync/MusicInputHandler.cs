using SymphonyFrameWork.Attribute;
using System;
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
        ///     初期化を行う。
        /// </summary>
        /// <param name="timeSignatures">入力によってなりえる拍子</param>
        public void Init(float[] timeSignatures)
        {
            _timeSignatures = timeSignatures.OrderBy(n => n).ToArray();
            _longestSignatureIndex = GetLongestSignatureIndex();
        }
        /// <summary>
        ///     入力されたタイミングを基いて、なりえる最も近い拍子を取得する。
        /// </summary>
        /// <param name="currentBeat">入力時点の全体拍数</param>
        /// <returns>最も近い拍子</returns>
        public float GetInputTimeSignature()
        {
            _debugLog.Clear(); // デバッグログをクリア
            _debugLog.AppendLine("=== Beat Input Debug ===");

            // 現在拍数
            double currentBeat = _musicBuffer.CurrentBeat;
            // 最後入力の標準拍タイミング。初回入力の場合は0とする
            double lastBeat = _inputedTimingList?.Count > 0 ? _inputedTimingList.Last() : 0;
            // 最後入力との拍数差
            double betweenBeat = currentBeat - lastBeat;
            // 最も近い拍子の配列インデックス
            int detectedTimeSignatureIndex = FindNearestTimeSignature(betweenBeat);
            // 最も近い標準拍タイミング（単位：拍数）
            double quantizedBeat = CalcQuantizedBeat(lastBeat, detectedTimeSignatureIndex);
            
            _debugLog.AppendLine($"Input Beat: {currentBeat:F3}, Last Beat: {lastBeat:F3}, Between: {betweenBeat:F3}");
            _debugLog.AppendLine($"Detected Beat Length: {GetBeatLength(detectedTimeSignatureIndex):F3} ({_timeSignatures[detectedTimeSignatureIndex]}拍子)");
            _debugLog.AppendLine($"Quantized Beat: {quantizedBeat:F3}");

            // 入力キューに今回入力と最も近い標準拍を記録する
            _inputedTimingList.Enqueue(quantizedBeat);
            // 古い入力を削除する
            DequeueOldInput(currentBeat, _inputHistoryBarLimit);

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
        [SerializeField, Range(0.5f, 1.0f)]
        private double _inputFixThreshold = 0.8d; // 入力タイミング追いかけ処理の補正閾値
        [SerializeField]
        int _inputHistoryBarLimit = 4; // 入力記録を最大何小節保持するか

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
        ///     指定された時間差に最も近い拍子のインデックスを取得する
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
                double diff = Math.Abs(timeDifference - beatLength);

                _debugLog.AppendLine($"TimeSignature {i}: {_timeSignatures[i]}拍子, BeatLength: {beatLength:F3}, Diff: {diff:F3}");

                // 入力時間差と拍子の1拍長さの差が増え始めたら結果が既に確定しているため、ループ終了
                if(diff > mostNearTimingValue)
                {
                    break;
                }

                if (diff < mostNearTimingValue)
                {
                    mostNearTimingIndex = i;
                    mostNearTimingValue = diff;
                }
            }
            return mostNearTimingIndex;
        }

        /// <summary>
        ///     指定された拍子インデックスの拍の長さを取得する
        /// </summary>
        /// <param name="timeSignatureIndex">拍子インデックス</param>
        /// <returns>拍の長さ</returns>
        private double GetBeatLength(int timeSignatureIndex) => _musicBuffer.PropTimeSignature / _timeSignatures[timeSignatureIndex];

        /// <summary>
        ///     1拍が最も長い拍子のIndexを取得する
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

            // 前回入力からの間隔が最長拍以上の場合、追いかけ処理を行う
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
            StringBuilder logStr = new StringBuilder($"最後の入力ズレ補正。補正前の結果：{ret}、現在拍：{_musicBuffer.CurrentBeat}");

            // 前の標準拍から入力までの長さが補正閾値を超えている場合、次の標準拍とする
            ret += _musicBuffer.CurrentBeat - ret > _inputFixThreshold ? 1d : 0d;

            logStr.AppendLine($"補正後の結果：{ret}");
            Debug.Log(logStr.ToString());
            return ret;
        }

        /// <summary>
        ///     入力キューから古い入力を削除する
        /// </summary>
        /// <param name="currentBeat">現在拍</param>
        /// <param name="barLimit">最大保持小節数</param>
        private void DequeueOldInput(double currentBeat, double barLimit)
        {
            // 残すか判定用の最小タイミング
            double dequeueThreshold = currentBeat - (currentBeat % _musicBuffer.PropTimeSignature) - _musicBuffer.PropTimeSignature * barLimit;
            // 記録された入力タイミング
            double inputedBeat;
            while (_inputedTimingList.TryPeek(out inputedBeat))
            {
                // 最小拍数より古い入力を削除し、最小拍数以降の入力まで削除したらループ終了
                if(inputedBeat < dequeueThreshold)
                {
                    _inputedTimingList.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }
        #endregion
    }
}