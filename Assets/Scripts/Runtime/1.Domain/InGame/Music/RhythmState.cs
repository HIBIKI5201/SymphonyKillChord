using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズム入力の履歴を管理するクラス。
    /// </summary>
    public class RhythmState
    {
        /// <summary>
        ///     指定された容量で履歴管理クラスを生成する。
        /// </summary>
        /// <param name="capacity"> バッファの容量。 </param>
        public RhythmState(int capacity)
        {
            _recordBuffer = new RingBuffer<RhythmInputRecord>(capacity);

            _beatTypeCache = new BeatType[capacity];
            _timingCache = new float[capacity];
            _actionTypeCache = new BattleActionType[capacity];
        }

        /// <summary> 現在の履歴数。 </summary>
        public int Count => _recordBuffer.Count;

        /// <summary> 最後に登録されたタイミング（unscaledTime）を取得する。 </summary>
        public float LastTiming => Count > 0 ? _recordBuffer.PeekLast().Timing : 0f;

        /// <summary>
        ///     計算済みの値をバッファに登録する。
        /// </summary>
        /// <param name="beatType"> 計算済みの拍子。 </param>
        /// <param name="timing"> 登録時のタイミング。 </param>
        /// <param name="actionType"> アクションの種類。 </param>
        public void Enqueue(BeatType beatType, float timing, BattleActionType actionType)
        {
            RhythmInputRecord record = new RhythmInputRecord(beatType, timing, actionType);
            _recordBuffer.Enqueue(record);
        }

        /// <summary>
        ///     履歴レコードをスパンとして取得する。
        /// </summary>
        /// <returns> レコードの読み取り専用スパン。 </returns>
        public ReadOnlySpan<RhythmInputRecord> GetHistoryRecord()
        {
            return _recordBuffer.AsReadonlySpan();
        }

        /// <summary>
        ///     拍の種類の履歴を古い順に取得する。
        /// </summary>
        /// <returns> 拍の種類の読み取り専用スパン。 </returns>
        public ReadOnlySpan<BeatType> GetHistoryBeatType()
        {
            ReadOnlySpan<RhythmInputRecord> records = _recordBuffer.AsReadonlySpan();

            for (int i = 0; i < records.Length; i++)
            {
                _beatTypeCache[i] = records[i].BeatType;
            }

            return _beatTypeCache.AsSpan(0, records.Length);
        }

        /// <summary>
        ///     タイミングの履歴を取得する。
        /// </summary>
        /// <returns> タイミングの読み取り専用スパン。 </returns>
        public ReadOnlySpan<float> GetHistoryTiming()
        {
            ReadOnlySpan<RhythmInputRecord> records = _recordBuffer.AsReadonlySpan();

            for (int i = 0; i < records.Length; i++)
            {
                _timingCache[i] = records[i].Timing;
            }

            return _timingCache.AsSpan(0, records.Length);
        }

        /// <summary>
        ///     アクション種類の履歴を取得する。
        /// </summary>
        /// <returns> アクション種類の読み取り専用スパン。 </returns>
        public ReadOnlySpan<BattleActionType> GetHistoryActionType()
        {
            ReadOnlySpan<RhythmInputRecord> records = _recordBuffer.AsReadonlySpan();

            for (int i = 0; i < records.Length; i++)
            {
                _actionTypeCache[i] = records[i].ActionType;
            }

            return _actionTypeCache.AsSpan(0, records.Length);
        }

        /// <summary>
        ///     履歴をクリアする。
        /// </summary>
        public void Clear()
        {
            _recordBuffer.Clear();
        }

        private readonly RingBuffer<RhythmInputRecord> _recordBuffer;

        private readonly BeatType[] _beatTypeCache;
        private readonly float[] _timingCache;
        private readonly BattleActionType[] _actionTypeCache;
    }
}