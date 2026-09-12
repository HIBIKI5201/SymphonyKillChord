namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///     リザルト画面から選択された遷移操作です。
    /// </summary>
    public enum StageResultExitAction
    {
        /// <summary> 帰還します。 </summary>
        Complete = 0,

        /// <summary> 同じステージへ再出撃します。 </summary>
        Retry = 1,
    }
}
