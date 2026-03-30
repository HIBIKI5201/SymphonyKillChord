using System;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズム入力の履歴を管理するクラス。
    /// </summary>
    public class RhythmState
    {
        public int Count => _typeBuffer.Count;

        private readonly RingBuffer<ActionType> _typeBuffer;
        private readonly RingBuffer<int> _beatTypeBuffer;
        private readonly RingBuffer<float> _timingBuffer;

        private RhythmDefinition _rhythmDefinition;

        public RhythmState(RhythmDefinition rhythmDefinition, int capacity)
        {
            _typeBuffer = new RingBuffer<ActionType>(capacity);
            _beatTypeBuffer = new RingBuffer<int>(capacity);
            _timingBuffer = new RingBuffer<float>(capacity);
            _rhythmDefinition = rhythmDefinition;
        }

        public double GetExecuteTime(ExecuteRequestTiming timing, double accurateBeat)
        {
            return _rhythmDefinition.GetExecuteTime(timing, accurateBeat);
        }

        /// <summary>
        ///     入力登録。
        /// </summary>
        public void RegisterActionQueue(ActionType type, float unscaledTime)
        {
            int signature = 1;

            if (Count != 0)
            {
                var lastTiming = _timingBuffer.PeekLast();
                var actionLength = unscaledTime - lastTiming;
                signature = _rhythmDefinition.GetNearestSignature(actionLength);
            }

            Enqueue(signature, unscaledTime, type);
        }

        public void Enqueue(ActionParams param)
        {
            Enqueue(param.BeatType, param.Timing, param.ActionType);
        }

        public void Enqueue(int beatType, float timing, ActionType actionType)
        {
            _beatTypeBuffer.Enqueue(beatType);
            _typeBuffer.Enqueue(actionType);
            _timingBuffer.Enqueue(timing);
        }

        /// <summary>
        /// 古い順に取得（Spanコピー）
        /// </summary>
        public ReadOnlySpan<int> GetHistoryBeatType()
        {
            return _beatTypeBuffer.AsReadonlySpan();
        }

        public ReadOnlySpan<float> GetHistoryTiming()
        {
            return _timingBuffer.AsReadonlySpan();
        }

        public ReadOnlySpan<ActionType> GetHistoryActionType()
        {
            return _typeBuffer.AsReadonlySpan();
        }
    }
}