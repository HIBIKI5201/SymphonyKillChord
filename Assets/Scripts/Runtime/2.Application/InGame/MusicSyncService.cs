using System;
using System.Threading;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using UnityEngine;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class MusicSyncService : IMusicSyncService
    {
        private const int BUFFER_SIZE = 64;

        private readonly RhythmState _rhythmState;
        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        public MusicSyncService(RhythmDefinition rhythmDefinition)
        {
            _rhythmState = new(rhythmDefinition, BUFFER_SIZE);
        }
        
        /// <summary>
        /// 毎フレーム処理。
        /// </summary>
        /// <param name="playTime">音楽の再生時間</param>
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
            var executeTime = _rhythmState.GetExecuteTime(timing, accurateBeat);
            _scheduledActions.Enqueue(new(action, ct), executeTime);
        }

        public void RegisterBattleActionHistory(ActionType actionType)
        {
            _rhythmState.RegisterActionQueue(actionType, Time.unscaledTime);
        }
    }
}