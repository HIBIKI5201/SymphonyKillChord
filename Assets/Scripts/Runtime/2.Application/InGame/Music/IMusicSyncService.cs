using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    public interface IMusicSyncService
    {
        void Update(double playTime);
        int GetHistoryLength();

        ReadOnlySpan<BeatType> GetBeatTypeHistory();
        ReadOnlySpan<float> GetBeatTypeTiming();
        ReadOnlySpan<BattleActionType> GetActionHistory();

        void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        BeatType GetCurrentBeatType(float unscaledTime);
        void RegisterBattleActionHistory(BattleActionType actionType, BeatType beatType, float unscaledTime);
    }
}