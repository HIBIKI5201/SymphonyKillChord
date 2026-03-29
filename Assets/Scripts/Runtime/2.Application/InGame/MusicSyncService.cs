using System;
using System.Threading;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class MusicSyncService : IMusicSyncService
    {
        private const int BUFFER_SIZE = 64;
        private readonly RhythmDefinition _rhythmDefinition;

        private readonly RhythmState _rhythmState;
        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        public MusicSyncService(RhythmDefinition rhythmDefinition)
        {
            _rhythmDefinition = rhythmDefinition;
            _rhythmState = new(_rhythmDefinition.Bpm, BUFFER_SIZE);
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

        public int GetHistoryLength()
        {
            return _rhythmState.Count;
        }

        public ReadOnlySpan<int> GetBeatTypeHistory()
        {
            return _rhythmState.GetHistoryBeatType();
        }

        public ReadOnlySpan<float> GetBeatTypeTiming()
        {
            return _rhythmState.GetHistoryTiming();
        }

        public ReadOnlySpan<ActionType> GetActionHistory()
        {
            return _rhythmState.GetHistoryActionType();
        }

        public void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct)
        {
            var executeTime = _rhythmDefinition.GetExecuteTime(timing, accurateBeat);
            _scheduledActions.Enqueue(new(action, ct), executeTime);
        }

        public void RegisterButtleActionHistory(ActionType actionType)
        {
            _rhythmState.RegisterActionQueue(actionType, Time.unscaledTime);
        }
    }
}