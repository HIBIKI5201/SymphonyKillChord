namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイセッションを再開する画面の種類です。
    /// </summary>
    public enum PlaySessionScene
    {
        /// <summary> 再開する画面がなく、タイトルから始めます。 </summary>
        None = 0,

        /// <summary> アウトゲーム（ホーム）から再開します。 </summary>
        OutGame = 1,

        /// <summary> 記録したシナリオステージの開始地点から再開します。 </summary>
        Scenario = 2,

        /// <summary> 記録したバトルステージの開始地点から再開します。 </summary>
        InGame = 3
    }
}
