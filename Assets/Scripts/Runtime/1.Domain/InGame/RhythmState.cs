using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain
{
    public class RhythmState
    {
        public int Count => _typeList.Count;

        private readonly List<ActionType> _typeList = new();
        private readonly List<int> _beatTypeList = new();
        private readonly List<float> _timing = new();
        private readonly int _bpm;

        public RhythmState(int bpm)
        {
            _bpm = bpm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="unscaledTime">Time.unscaledTime</param>
        public void RegisterActionQueue(ActionType type, float unscaledTime)
        {
            int signature = 1;
            if (_typeList.Count != 0)
            {
                var actionLength =
                    unscaledTime - _timing[^1];
                signature = GetNearestSignature(actionLength);
            }

            _typeList.Add(type);
            _timing.Add(unscaledTime);
            _beatTypeList.Add(signature);
        }

        public IReadOnlyList<int> GetHistoryBeatType()
        {
            return _beatTypeList;
        }

        public IReadOnlyList<float> GetHistoryTiming()
        {
            return _timing;
        }

        public IReadOnlyList<ActionType> GetHistoryActionType()
        {
            return _typeList;
        }

        public void Enqueue(ActionParams param)
        {
            _beatTypeList.Add(param.BeatType);
            _typeList.Add(param.ActionType);
            _timing.Add(param.Timing);
        }

        public void Enqueue(int beatType, float timing, ActionType actionType)
        {
            _beatTypeList.Add(beatType);
            _typeList.Add(actionType);
            _timing.Add(timing);
        }

        public ActionParams Dequeue()
        {
            if (_typeList.Count <= 0) return default;

            var returnParam = new ActionParams(_typeList[0], _beatTypeList[0], _timing[0]);
            _typeList.RemoveAt(0);
            _beatTypeList.RemoveAt(0);
            _timing.RemoveAt(0);
            
            return returnParam;
        }

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
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