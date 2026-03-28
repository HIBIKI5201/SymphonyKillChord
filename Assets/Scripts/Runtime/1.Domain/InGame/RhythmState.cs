using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain
{
    public class RhythmState
    {
        public ActionParams LastAction => _actionList[^1];
        public ActionParams Peek => _actionList[0];
        public int Count => _actionList.Count;

        private readonly List<ActionParams> _actionList = new();
        private int _bpm;

        public RhythmState(int bpm)
        {
            _bpm = bpm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="unscaledTime">Time.unscaledTime</param>
        private void RegisterActionQueue(ActionType type, float unscaledTime)
        {
            int signature = 1;
            if (_actionList.Count != 0)
            {
                var actionLength =
                    unscaledTime - _actionList[^1].Timing;
                signature = GetNearestSignature(actionLength);
            }

            var param = new ActionParams(type, signature);

            _actionList.Add(param);
        }

        private void Enqueue(ActionParams param)
        {
            _actionList.Add(param);
        }

        private ActionParams Dequeue()
        {
            if (_actionList.Count <= 0) return default;

            var returnParam = _actionList[0];
            _actionList.RemoveAt(0);
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