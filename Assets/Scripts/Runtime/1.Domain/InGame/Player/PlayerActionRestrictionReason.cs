using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Player
{
    /// <summary>
    ///     プレイヤーの行動を制限する理由。
    /// </summary>
    public enum PlayerActionRestrictionReason
    {
        None,
        Tutorial,
        Silence
    }
}
