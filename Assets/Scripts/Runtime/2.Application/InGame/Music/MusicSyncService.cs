using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Music
{
    public class MusicSyncService : IMusicSyncService
    {
        private const int BUFFER_SIZE = 64;

        private readonly RhythmState _rhythmState;
        private readonly RhythmDefinition _rhythmDefinition;
        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        public MusicSyncService(RhythmDefinition rhythmDefinition)
        {
            _rhythmState = new(BUFFER_SIZE);
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

        public int GetHistoryLength()
        {
            return _rhythmState.Count;
        }

        public int GetCurrentBeatType(float unscaledTime)
        {
            if (_rhythmState.Count == 0) return 1;

            float lastTime = _rhythmState.LastTiming;
            double duration = (double)(unscaledTime - lastTime);

            return _rhythmDefinition.CalculateBeatType(duration);
        }

        public ReadOnlySpan<int> GetBeatTypeHistory()
        {
            return _rhythmState.GetHistoryBeatType();
        }

        public ReadOnlySpan<float> GetBeatTypeTiming()
        {
            return _rhythmState.GetHistoryTiming();
        }

        public ReadOnlySpan<BattleActionType> GetActionHistory()
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

        public void RegisterBattleActionHistory(BattleActionType actionType, int beatType, float unscaledTime)
        {
            _rhythmState.Enqueue(beatType, unscaledTime, actionType);
        }
    }
}