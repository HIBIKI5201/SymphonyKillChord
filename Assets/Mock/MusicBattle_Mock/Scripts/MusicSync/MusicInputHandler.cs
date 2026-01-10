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
    public class MusicInputHandler
    {
        #region コンストラクタ
        /// <summary>
        ///     初期化を行う。
        /// </summary>
        /// <param name="timeSignatures"> 入力によってなりえる拍子。 </param>
        public MusicInputHandler(CriMusicBuffer buffer, MusicSyncConfigs configs, SignatureDatabase timeSignatures)
        {
            _musicBuffer = buffer;
            _configs = configs;
            _timeSignatures = timeSignatures.SignatureDataSpan.Select(e => e.Signature).OrderBy(n => n).ToArray();
            _longestSignatureIndex = GetLongestSignatureIndex();
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     入力されたタイミングを基いて、なりえる最も近い拍子を取得する。
        /// </summary>
        /// <param name="currentBeat"> 入力時点の全体拍数。 </param>
        /// <returns> 最も近い拍子。 </returns>
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

            _debugLog.AppendLine($"bpm:{_musicBuffer.CurrentBpm}, beat:{_musicBuffer.CurrentBeat}");
            _debugLog.AppendLine($"Input Beat: {currentBeat:F3}, Last Beat: {lastBeat:F3}, Between: {betweenBeat:F3}");
            _debugLog.AppendLine($"Detected Beat Length: {GetBeatLength(detectedTimeSignatureIndex):F3} ({_timeSignatures[detectedTimeSignatureIndex]}拍子)");
            _debugLog.AppendLine($"Quantized Beat: {quantizedBeat:F3}");

            // 入力キューに今回入力と最も近い標準拍を記録する
            _inputedTimingList.Enqueue(quantizedBeat);
            // 古い入力を削除する
            DequeueOldInput(currentBeat, _configs.InputHistoryBarLimit);

            _debugLog.AppendLine($"Inputed Timing List: {string.Join(", ", _inputedTimingList.Select(b => b.ToString("F3")))}"); // 表示フォーマットは小数点以下3桁

            // 検出された拍子を履歴に追加し、古いものを削除する
            _signatureHistory.Enqueue(_timeSignatures[detectedTimeSignatureIndex]);
            while (_signatureHistory.Count > _configs.InputSignatureHistoryLimit)
            {
                _signatureHistory.Dequeue();
            }
            _debugLog.AppendLine($"Signature History: {string.Join(", ", _signatureHistory.Select(s => s.ToString("F0")))}");

            // すべてのデバッグ情報を一括で出力
            Debug.Log(_debugLog.ToString());
            return _timeSignatures[detectedTimeSignatureIndex];
        }

        /// <summary>
        ///     入力された拍子の履歴を取得します。
        /// </summary>
        /// <returns>入力された拍子の履歴。</returns>
        public ReadOnlySpan<float> GetSignatureHistory()
        {
            return new ReadOnlySpan<float>(_signatureHistory.ToArray());
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> CRI ADXの音楽バッファを管理するクラスの参照。 </summary>
        private readonly CriMusicBuffer _musicBuffer;
        /// <summary> 音楽同期に関するコンフィグ。 </summary>
        private readonly MusicSyncConfigs _configs;
        /// <summary> 入力によって検出されうる拍子のリスト。昇順でソートされている。 </summary>
        private readonly float[] _timeSignatures;
        #endregion

        #region プライベートフィールド
        /// <summary> 入力された標準拍のタイミングを記録するキュー。 </summary>
        private Queue<double> _inputedTimingList = new();
        /// <summary> 入力された拍子の履歴を記録するキュー。 </summary>
        private Queue<float> _signatureHistory = new();
        /// <summary> 設定された拍子リストの中で、最も1拍の時間が長い拍子のインデックス。 </summary>
        private int _longestSignatureIndex;
        /// <summary> デバッグログ情報を構築するためのStringBuilder。 </summary>
        private readonly StringBuilder _debugLog = new StringBuilder();
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     オブジェクトが有効になったときに一度だけ呼び出されます。
        ///     ビートベースのクオンタイズ設定で音楽入力が初期化されたことをログに出力します。
        /// </summary>
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

                // 入力時間差と拍子の1拍長さの差が増え始めたら結果が既に確定しているため、ループ終了。
                if (diff > mostNearTimingValue)
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
        ///     指定された拍子インデックスの拍の長さを取得する。
        /// </summary>
        /// <param name="timeSignatureIndex"> 拍子インデックス。 </param>
        /// <returns>拍の長さ</returns>
        private double GetBeatLength(int timeSignatureIndex) => _musicBuffer.PropTimeSignature / _timeSignatures[timeSignatureIndex];

        /// <summary>
        ///     1拍が最も長い拍子のIndexを取得する。
        /// </summary>
        /// <returns> 拍子リストのIndex。 </returns>
        private int GetLongestSignatureIndex()
        {
            // _timeSignaturesは昇順でソートされているため、インデックス0が最も小さい拍子値（短い拍長）、
            // 最後のインデックスが最も大きい拍子値（長い拍長）となる。
            // ここでは最も「長い」拍の長さを持つ拍子を取得するため、拍子値が最も小さいもののインデックスを返す。
            return _timeSignatures[0] > _timeSignatures[_timeSignatures.Length - 1] ?
                _timeSignatures.Length - 1 : 0;
        }

        /// <summary>
        ///     入力の標準拍タイミングを算出する
        ///     前回入力からの間隔が最も長い拍子のを超えている場合、補正計算を行い、
        ///     BGMの固有拍子に基づき、直近の標準拍タイミングを計算結果とする
        /// </summary>
        /// <param name="lastBeat">前回入力タイミング</param>
        /// <param name="signatureIndex">最も近い拍子のリストIndex</param>
        /// <returns>BGM再生開始から今回入力直近までの長さ</returns>
        private double CalcQuantizedBeat(double lastBeat, int signatureIndex)
        {
            // 前回入力からの間隔が最長拍未満の場合、前回入力 + 検出拍子長さとする。
            // 検出された拍子の長さ（拍数単位）
            double beatLength = GetBeatLength(signatureIndex);
            if (signatureIndex != _longestSignatureIndex)
            {
                return lastBeat + beatLength;
            }

            // 前回入力からの間隔が最長拍以上の場合、追いかけ処理を行う。
            double ret = lastBeat;
            bool loopEndFlg = false;
            while (!loopEndFlg)
            {
                // 前回入力タイミングから、最長拍子の拍長を1回ずつ、超えない所まで加算する。
                if (ret + beatLength < _musicBuffer.CurrentBeat)
                {
                    ret += beatLength;
                }
                else
                {
                    // そしてBGM固有拍子の拍長を1拍分ずつ加算し、入力時間直前の標準拍タイミングを算出する。
                    while (!loopEndFlg)
                    {
                        if (ret + 1d < _musicBuffer.CurrentBeat)
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
            // 最後に、入力ずれを補正する。入力が次の拍に近い場合、結果を次の拍とする。
            StringBuilder logStr = new StringBuilder($"最後の入力ズレ補正。補正前の結果：{ret}、現在拍：{_musicBuffer.CurrentBeat}");

            // 前の標準拍から入力までの長さが補正閾値を超えている場合、次の標準拍とする。
            ret += _musicBuffer.CurrentBeat - ret > _configs.InputFixThreshold ? 1d : 0d;

            logStr.AppendLine($"補正後の結果：{ret}");
            Debug.Log(logStr.ToString());
            return ret;
        }

        /// <summary>
        ///     入力キューから古い入力を削除する。
        /// </summary>
        /// <param name="currentBeat">現在拍</param>
        /// <param name="barLimit">最大保持小節数</param>
        private void DequeueOldInput(double currentBeat, double barLimit)
        {
            // 残すか判定用の最小タイミング。
            double dequeueThreshold = currentBeat - (currentBeat % _musicBuffer.PropTimeSignature) - _musicBuffer.PropTimeSignature * barLimit;
            // 記録された入力タイミング。
            double inputedBeat;
            while (_inputedTimingList.TryPeek(out inputedBeat))
            {
                // 最小拍数より古い入力を削除し、最小拍数以降の入力まで削除したらループ終了。
                if (inputedBeat < dequeueThreshold)
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
