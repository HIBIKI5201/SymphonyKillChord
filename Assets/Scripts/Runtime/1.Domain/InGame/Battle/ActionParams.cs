using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     バトルアクションの種類やタイミングを保持する構造体。
    /// </summary>
    public readonly struct ActionParams
    {
        public ActionParams(BattleActionType actionType, int beatType)
        {
            BeatType = beatType;
            ActionType = actionType;
            Timing = Time.unscaledTime;
        }

        public ActionParams(BattleActionType actionType, int beatType, float timing)
        {
            BeatType = beatType;
            ActionType = actionType;
            Timing = timing;
        }

        public readonly BattleActionType ActionType;
        public readonly int BeatType;
        public readonly float Timing;
    }
}