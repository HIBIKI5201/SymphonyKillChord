using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズム入力の記録を保持する構造体。
    /// </summary>
    public readonly struct RhythmInputRecord
    {
        /// <summary>
        ///     新しい入力記録を生成する。
        /// </summary>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="timing"> タイミング。 </param>
        /// <param name="actionType"> アクションの種類。 </param>
        /// <param name="isJust"> Justで入力したかどうか。 </param>
        public RhythmInputRecord(BeatType beatType, float timing, BattleActionType actionType, bool isJust = false)
        {
            BeatType = beatType;
            Timing = timing;
            ActionType = actionType;
            IsJust = isJust;
        }

        /// <summary> 判定された拍子。 </summary>
        public BeatType BeatType { get; }

        /// <summary> 入力時に使用した時計のタイムスタンプ。 </summary>
        public float Timing { get; }

        /// <summary> アクションの種類。 </summary>
        public BattleActionType ActionType { get; }

        /// <summary> Justで入力したかどうか。 </summary>
        public bool IsJust { get; }
    }
}
