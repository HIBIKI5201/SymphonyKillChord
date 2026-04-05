namespace Research.SaveSystem
{
    /// <summary>
    ///     ロードエラー時のイベント
    /// </summary>
    public readonly struct EOnLoadError : IEvent
    {
        public EOnLoadError(string message)
        {
            ErrorMessage = message;
        }
        /// <summary>表示用のエラーメッセージ</summary>
        public readonly string ErrorMessage;
    }
}
