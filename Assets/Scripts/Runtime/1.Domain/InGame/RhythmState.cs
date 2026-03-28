using System;

namespace KillChord.Runtime.Domain
{
    public class RhythmState
    {
        public int Count => _count;

        private readonly ActionType[] _typeBuffer;
        private readonly int[] _beatTypeBuffer;
        private readonly float[] _timingBuffer;

        private int _head;   // 先頭
        private int _count;  // 要素数

        private readonly int _capacity;
        private readonly int _bpm;

        public RhythmState(int bpm, int capacity = 64)
        {
            _bpm = bpm;
            _capacity = capacity;

            _typeBuffer = new ActionType[capacity];
            _beatTypeBuffer = new int[capacity];
            _timingBuffer = new float[capacity];
        }

        /// <summary>
        /// 末尾インデックス取得
        /// </summary>
        private int TailIndex => (_head + _count) % _capacity;

        /// <summary>
        /// インデックス変換
        /// </summary>
        private int ToPhysicalIndex(int index)
        {
            return (_head + index) % _capacity;
        }

        public void RegisterActionQueue(ActionType type, float unscaledTime)
        {
            int signature = 1;

            if (_count != 0)
            {
                int lastIndex = ToPhysicalIndex(_count - 1);
                var actionLength = unscaledTime - _timingBuffer[lastIndex];
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
            int index = TailIndex;

            _beatTypeBuffer[index] = beatType;
            _typeBuffer[index] = actionType;
            _timingBuffer[index] = timing;

            if (_count == _capacity)
            {
                // 上書き
                _head = (_head + 1) % _capacity;
            }
            else
            {
                _count++;
            }
        }

        public ActionParams Dequeue()
        {
            if (_count == 0) return default;

            int index = _head;

            var result = new ActionParams(
                _typeBuffer[index],
                _beatTypeBuffer[index],
                _timingBuffer[index]);

            _head = (_head + 1) % _capacity;
            _count--;

            return result;
        }

        /// <summary>
        /// Spanで履歴取得
        /// </summary>
        public ReadOnlySpan<int> GetHistoryBeatType(Span<int> buffer)
        {
            for (int i = 0; i < _count; i++)
            {
                buffer[i] = _beatTypeBuffer[ToPhysicalIndex(i)];
            }
            return buffer.Slice(0, _count);
        }

        public ReadOnlySpan<float> GetHistoryTiming(Span<float> buffer)
        {
            for (int i = 0; i < _count; i++)
            {
                buffer[i] = _timingBuffer[ToPhysicalIndex(i)];
            }
            return buffer.Slice(0, _count);
        }

        public ReadOnlySpan<ActionType> GetHistoryActionType(Span<ActionType> buffer)
        {
            for (int i = 0; i < _count; i++)
            {
                buffer[i] = _typeBuffer[ToPhysicalIndex(i)];
            }
            return buffer.Slice(0, _count);
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