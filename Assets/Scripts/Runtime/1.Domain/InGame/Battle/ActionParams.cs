using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     バトルアクションの種類やタイミングを保持する構造体。
    /// </summary>
    public readonly struct ActionParams
    {
        /// <summary>
        ///     アクションタイプとビートタイプを指定して初期化するコンストラクタ。
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="beatType"></param>
        public ActionParams(BattleActionType actionType, int beatType)
        {
            BeatType = beatType;
            ActionType = actionType;
            Timing = Time.unscaledTime;
        }

        /// <summary>
        ///     アクションタイプ、ビートタイプ、タイミングを指定して初期化するコンストラクタ。
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="beatType"></param>
        /// <param name="timing"></param>
        public ActionParams(BattleActionType actionType, int beatType, float timing)
        {
            BeatType = beatType;
            ActionType = actionType;
            Timing = timing;
        }

        /// <summary> アクションの種類を取得する。 </summary>
        public readonly BattleActionType ActionType;

        /// <summary> ビートの種類を取得する。 </summary>
        public readonly int BeatType;

        /// <summary> アクションが発生したタイミングを取得する。 </summary>
        public readonly float Timing;
    }
}