using System;
using System.Collections.Generic;
using System.Threading;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class MusicSyncService : IMusicSyncService
    {
        public event Action<ActionParams> OnActionTriggered;

        private readonly RhythmDefinition _rhythmDefinition;

        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        public MusicSyncService(RhythmDefinition rhythmDefinition)
        {
            _rhythmDefinition = rhythmDefinition;
        }

        public void Update(double playTime)
        {
            while (_scheduledActions.TryPeek(out var actionData, out double executeTime))
            {
                if (actionData.CancellationToken.IsCancellationRequested)
                {
                    _scheduledActions.Dequeue();
                    continue;
                }

                if (executeTime <= playTime)
                {
                    _scheduledActions.Dequeue();
                    actionData.Action?.Invoke();
                    continue;
                }

                break;
            }
        }

        public void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct)
        {
            var executeTime = GetExecuteTime(timing, accurateBeat);
            _scheduledActions.Enqueue(new(action, ct), executeTime);
        }

        public void RegisterButtleActionHistory(ActionType actionType)
        {
        }

        public void ClearActions()
        {
        }

        private double GetExecuteTime(ExecuteRequestTiming timing, double accurateBeat)
        {
            if (_rhythmDefinition.Bpm <= 0) return 0;
            const double propTimeSignature = 4d;
            double currentBar = Math.Floor(accurateBeat / propTimeSignature);
            double targetBar = currentBar + timing.BarFlag;

            double barLengthMs = _rhythmDefinition.BeatLength * propTimeSignature;
            double targetBarStartTimingMs = targetBar * barLengthMs;
            double offsetInBarMs = (barLengthMs / timing.Beat.Signature) * (timing.Beat.Count - 1);
            return targetBarStartTimingMs + offsetInBarMs;
        }

        #region 後にドメインに移植

        public ActionParams LastAction => _actionList[^1];
        public ActionParams Peek => _actionList[0];
        public int Count => _actionList.Count;

        private readonly List<ActionParams> _actionList = new();

        private void RegisterActionQueue(ActionType type)
        {
            int signature = 1;
            if (_actionList.Count != 0)
            {
                var actionLength =
                    Time.unscaledTime - _actionList[^1].Timing;
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
            if (_rhythmDefinition.Bpm <= 0) return 4;

            double beatSeconds = 60d / _rhythmDefinition.Bpm;
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

        #endregion
    }
}