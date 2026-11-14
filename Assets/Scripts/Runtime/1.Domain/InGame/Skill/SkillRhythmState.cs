using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Collections;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     プレイヤーの入力履歴保持クラス。
    /// </summary>
    public class SkillRhythmState
    {
        /// <summary>
        ///     入力履歴の保持数を指定して生成する。
        ///     0 以下の場合は例外を投げる。
        /// </summary>
        public SkillRhythmState(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be greater than 0.");
            }

            _recordBuffer = new RingBuffer<RhythmInputRecord>(capacity);

            _beatTypeCache = new BeatType[capacity];
            _timingCache = new float[capacity];
            _actionTypeCache = new BattleActionType[capacity];
        }
        /// <summary> 現在の履歴数。 </summary>
        public int Count => _recordBuffer.Count;

        /// <summary> 最後に登録された音源再生タイミングを取得する。 </summary>
        public float LastTiming => Count > 0 ? _recordBuffer.PeekLast().Timing : 0f;

        /// <summary>
        ///     計算済みの値をバッファに登録する。
        /// </summary>
        /// <param name="beatType"> 計算済みの拍子。 </param>
        /// <param name="timing"> 登録時の音源再生タイミング。 </param>
        /// <param name="actionType"> アクションの種類。 </param>
        /// <param name="isJust"> Justで入力したかどうか。 </param>
        public void Enqueue(BeatType beatType, float timing, BattleActionType actionType, bool isJust = false)
        {
            RhythmInputRecord record = new RhythmInputRecord(beatType, timing, actionType, isJust);
            _recordBuffer.Enqueue(record);
        }

        /// <summary>
        ///     新しい方から指定した数の入力のうち、Justで入力した数を数える。
        /// </summary>
        /// <param name="recentCount"> 数える入力の数。履歴の数より多い場合は履歴の全てを数える。 </param>
        /// <returns> Justで入力した数。 </returns>
        public int CountRecentJust(int recentCount)
        {
            ReadOnlySpan<RhythmInputRecord> records = _recordBuffer.AsReadonlySpan();
            int start = Math.Max(0, records.Length - recentCount);
            int justCount = 0;
            for (int i = start; i < records.Length; i++)
            {
                if (records[i].IsJust)
                {
                    justCount++;
                }
            }

            return justCount;
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
