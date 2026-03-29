using System;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.Domain
{
    public class RhythmState
    {
        public int Count => _typeBuffer.Count;

        private readonly RingBuffer<ActionType> _typeBuffer;
        private readonly RingBuffer<int> _beatTypeBuffer;
        private readonly RingBuffer<float> _timingBuffer;

        private readonly int _bpm;

        public RhythmState(int bpm, int capacity)
        {
            _bpm = bpm;

            _typeBuffer = new RingBuffer<ActionType>(capacity);
            _beatTypeBuffer = new RingBuffer<int>(capacity);
            _timingBuffer = new RingBuffer<float>(capacity);
        }

        /// <summary>
        /// 入力登録
        /// </summary>
        public void RegisterActionQueue(ActionType type, float unscaledTime)
        {
            int signature = 1;

            if (Count != 0)
            {
                var lastTiming = _timingBuffer.PeekLast();
                var actionLength = unscaledTime - lastTiming;
                signature = GetNearestSignature(actionLength);
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

        private int GetNearestSignature(double seconds)
        {
            if (_bpm <= 0) return 4;

            double beatSeconds = 60d / _bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }
    }
}