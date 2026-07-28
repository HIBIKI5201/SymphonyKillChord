namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの終了理由を表す列挙型。
    /// </summary>
    public enum MissionEndReason 
    {
        /// <summary> 終了していない。 </summary>
        None = 0,
        /// <summary> クリア。 </summary>
        Clear = 1,
        /// <summary> 失敗。 </summary>
        Fail = 2
    }
}
