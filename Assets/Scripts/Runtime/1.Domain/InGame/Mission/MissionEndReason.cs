using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの終了理由を表す列挙型。
    /// </summary>
    public enum MissionEndReason 
    {
        None = 0,
        Clear = 1,
        Fail = 2
    }
}