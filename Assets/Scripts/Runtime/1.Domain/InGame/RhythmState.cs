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

        public RhythmState(int bpm, int capacity = 64)
        {
            _bpm = bpm;

            _typeBuffer = new RingBuffer<ActionType>(capacity);
            _beatTypeBuffer = new RingBuffer<int>(capacity);
            _timingBuffer = new RingBuffer<float>(capacity);
        }

        /// <summary>
        /// 入力登録（拍計算込み）
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

        public ActionParams Dequeue()
        {
            if (Count == 0) return default;

            // PeekFirstで取得 → Clearではなく「論理削除」はできないため
            // RingBufferはDequeue未実装なので「先頭を進めるAPI」が必要
            // → 今回は簡易的に「擬似Dequeue」とする

            var result = new ActionParams(
                _typeBuffer.PeekFirst(),
                _beatTypeBuffer.PeekFirst(),
                _timingBuffer.PeekFirst()
            );

            // 注意：RingBufferにDequeueが無いため本来は追加実装が必要
            // 仮に「1個消費した扱い」にするなら別途インターフェース設計が必要

            throw new NotImplementedException("RingBufferにDequeue機能を追加してください");

            // return result;
        }

        /// <summary>
        /// 古い順に取得（Spanコピー）
        /// </summary>
        public ReadOnlySpan<int> GetHistoryBeatType(Span<int> buffer)
        {
            for (int i = 0; i < Count; i++)
            {
                buffer[i] = _beatTypeBuffer.PeekFirst(i);
            }

            return buffer.Slice(0, Count);
        }

        public ReadOnlySpan<float> GetHistoryTiming(Span<float> buffer)
        {
            for (int i = 0; i < Count; i++)
            {
                buffer[i] = _timingBuffer.PeekFirst(i);
            }

            return buffer.Slice(0, Count);
        }

        public ReadOnlySpan<ActionType> GetHistoryActionType(Span<ActionType> buffer)
        {
            for (int i = 0; i < Count; i++)
            {
                buffer[i] = _typeBuffer.PeekFirst(i);
            }

            return buffer.Slice(0, Count);
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