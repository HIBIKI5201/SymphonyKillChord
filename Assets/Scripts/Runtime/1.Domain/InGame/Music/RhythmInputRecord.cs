using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.Music
{
    public readonly struct RhythmInputRecord
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public RhythmInputRecord(BeatType beatType, float timing, BattleActionType actionType)
        {
            BeatType = beatType;
            Timing = timing;
            ActionType = actionType;
        }

        /// <summary> 判定された拍子。 </summary>
        public BeatType BeatType { get; }

        /// <summary> 入力タイミング（unscaledTime）。 </summary>
        public float Timing { get; }

        /// <summary> アクションの種類。 </summary>
        public BattleActionType ActionType { get; }
    }
}