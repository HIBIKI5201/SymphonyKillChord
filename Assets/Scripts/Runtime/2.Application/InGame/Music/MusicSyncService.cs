using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    /// 音楽同期のタイミング管理やアクション実行を行うサービス。
    /// </summary>
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

        /// <summary>
        /// 現在のタイミングが何拍子相当かを取得する。Time.unscaledTimeを利用する
        /// </summary>
        public int GetCurrentBeatType()
        {
            if (_rhythmState.Count == 0) return 1;

            float currentTime = Time.unscaledTime;
            float lastTime = _rhythmState.LastTiming;
            double duration = currentTime - lastTime;

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

        /// <summary>
        /// 計算済みの拍子を履歴に登録する。
        /// </summary>
        public void RegisterBattleActionHistory(BattleActionType actionType, int beatType)
        {
            _rhythmState.Enqueue(beatType, Time.unscaledTime, actionType);
        }
    }
}