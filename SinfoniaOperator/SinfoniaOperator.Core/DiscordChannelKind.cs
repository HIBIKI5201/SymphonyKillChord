namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     送信先のDiscordチャンネルを表す論理名。
    ///     呼び出し側はチャンネルIDを直接扱わず、この列挙型で送信先を指定する。
    /// </summary>
    public enum DiscordChannelKind
    {
        Task,
        TaskAlert,
        Sprint,
        WorkLog,
    }
}
