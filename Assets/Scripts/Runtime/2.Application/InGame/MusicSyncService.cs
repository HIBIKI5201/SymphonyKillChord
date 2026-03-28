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
    }
}