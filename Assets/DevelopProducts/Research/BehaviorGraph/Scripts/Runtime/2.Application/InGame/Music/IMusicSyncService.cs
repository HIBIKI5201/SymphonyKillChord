using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music
{
    public interface IMusicSyncService
    {
        void Update(double playTime);
        int GetHistoryLength();

        ReadOnlySpan<int> GetBeatTypeHistory();
        ReadOnlySpan<float> GetBeatTypeTiming();
        ReadOnlySpan<BattleActionType> GetActionHistory();

        void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        int GetCurrentBeatType(float unscaledTime);
        void RegisterBattleActionHistory(BattleActionType actionType, int beatType, float unscaledTime);
    }
}