using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    /// リズム入力の履歴を管理するクラス。
    /// </summary>
    public class RhythmState
    {
        public int Count => _typeBuffer.Count;

        /// <summary>
        /// 最後に登録されたタイミング（unscaledTime）を取得する。
        /// </summary>
        public float LastTiming => Count > 0 ? _timingBuffer.PeekLast() : 0f;

        private readonly RingBuffer<BattleActionType> _typeBuffer;
        private readonly RingBuffer<BeatType> _beatTypeBuffer;
        private readonly RingBuffer<float> _timingBuffer;

        public RhythmState(int capacity)
        {
            _typeBuffer = new RingBuffer<BattleActionType>(capacity);
            _beatTypeBuffer = new RingBuffer<BeatType>(capacity);
            _timingBuffer = new RingBuffer<float>(capacity);
        }

        /// <summary>
        ///  計算済みの値をバッファに登録する。
        /// </summary>
        /// <param name="beatType">計算済みの拍子</param>
        /// <param name="timing">登録時のタイミング(Time.unscaledTimeなど)</param>
        /// <param name="actionType">アクションの種類</param>
        public void Enqueue(BeatType beatType, float timing, BattleActionType actionType)
        {
            _beatTypeBuffer.Enqueue(beatType);
            _typeBuffer.Enqueue(actionType);
            _timingBuffer.Enqueue(timing);
        }

        /// <summary> 古い順に取得</summary>
        public ReadOnlySpan<BeatType> GetHistoryBeatType()
        {
            return _beatTypeBuffer.AsReadonlySpan();
        }

        public ReadOnlySpan<float> GetHistoryTiming()
        {
            return _timingBuffer.AsReadonlySpan();
        }

        public ReadOnlySpan<BattleActionType> GetHistoryActionType()
        {
            return _typeBuffer.AsReadonlySpan();
        }
    }
}