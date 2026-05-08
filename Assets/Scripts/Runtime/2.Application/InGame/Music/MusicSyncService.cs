using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     音楽との同期およびアクションの実行予約を管理するサービス。
    /// </summary>
    public class MusicSyncService : IMusicSyncService
    {
        /// <summary>
        ///     新しいサービスを生成する。
        /// </summary>
        /// <param name="rhythmDefinition"> リズムの定義。 </param>
        public MusicSyncService(RhythmDefinition rhythmDefinition)
        {
            _rhythmState = new(BUFFER_SIZE);
            _rhythmDefinition = rhythmDefinition;
            _scheduledActions = new PriorityQueue<ScheduledAction, double>();
        }

        /// <summary>
        ///     毎フレームの更新処理を行い、予約されたアクションを実行する。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
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

        /// <summary>
        ///     履歴の長さを取得する。
        /// </summary>
        /// <returns> 履歴の数。 </returns>
        public int GetHistoryLength()
        {
            return _rhythmState.Count;
        }

        /// <summary>
        ///     現在のタイミングにおける拍の種類を取得する。
        /// </summary>
        /// <param name="unscaledTime"> 現在の時間。 </param>
        /// <returns> 拍の種類。 </returns>
        public BeatType GetCurrentBeatType(float unscaledTime)
        {
            if (_rhythmState.Count == 0) return BeatType.One;

            float lastTime = _rhythmState.LastTiming;
            double duration = unscaledTime - lastTime;

            return _rhythmDefinition.CalculateBeatType(duration);
        }

        /// <summary>
        ///     拍の種類の履歴を取得する。
        /// </summary>
        /// <returns> 拍の種類の読み取り専用スパン。 </returns>
        public ReadOnlySpan<BeatType> GetBeatTypeHistory()
        {
            return _rhythmState.GetHistoryBeatType();
        }

        /// <summary>
        ///     拍のタイミングの履歴を取得する。
        /// </summary>
        /// <returns> タイミングの読み取り専用スパン。 </returns>
        public ReadOnlySpan<float> GetBeatTypeTiming()
        {
            return _rhythmState.GetHistoryTiming();
        }

        /// <summary>
        ///     アクションの履歴を取得する。
        /// </summary>
        /// <returns> アクション種類の読み取り専用スパン。 </returns>
        public ReadOnlySpan<BattleActionType> GetActionHistory()
        {
            return _rhythmState.GetHistoryActionType();
        }

        /// <summary>
        ///     将来の特定のタイミングで実行するアクションを予約する。
        /// </summary>
        /// <param name="accurateBeat"> 現在の正確な拍。 </param>
        /// <param name="timing"> 実行するタイミング指定。 </param>
        /// <param name="action"> 実行するアクション。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        public void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct)
        {
            double executeTime = MusicTimingCalculator.CalculateExecutionTime(_rhythmDefinition, timing, accurateBeat);
            _scheduledActions.Enqueue(new ScheduledAction(action, ct), executeTime);
        }

        /// <summary>
        ///     バトルアクションの履歴を登録する。
        /// </summary>
        /// <param name="actionType"> アクションの種類。 </param>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="unscaledTime"> 登録時間。 </param>
        public void RegisterBattleActionHistory(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            _rhythmState.Enqueue(beatType, unscaledTime, actionType);
        }

        /// <summary>
        ///     現在の小節内の進捗を取得する。
        /// </summary>
        /// <param name="unscaledTime"> 現在の時間。 </param>
        /// <returns> 0〜1の進捗。 </returns>
        public float GetBarProgress(float unscaledTime)
        {
            if (_rhythmState.Count == 0) return 0f;

            float lastTime = _rhythmState.LastTiming;
            float duration = unscaledTime - lastTime;

            return (float)_rhythmDefinition.CalculateElapsedBarCount(duration);
        }

        private const int BUFFER_SIZE = 64;

        private readonly RhythmState _rhythmState;
        private readonly RhythmDefinition _rhythmDefinition;
        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

    }
}