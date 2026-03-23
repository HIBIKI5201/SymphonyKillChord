namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブエラー時のイベント
    /// </summary>
    public readonly struct EOnSaveError : IEvent
    {
        public EOnSaveError(string msg)
        {
            ErrorMessage = msg;
        }
        /// <summary>表示用のエラーメッセージ</summary>
        public readonly string ErrorMessage;
    }
}
