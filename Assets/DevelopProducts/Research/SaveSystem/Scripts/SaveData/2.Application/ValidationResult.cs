namespace Research.SaveSystem
{
    /// <summary>
    ///     データ検証結果構造体。
    /// </summary>
    public readonly struct ValidationResult
    {
        /// <summary>検証結果（正常／異常）</summary>
        public readonly bool Result;
        /// <summary>結果メッセージ</summary>
        public readonly string Message;
        public ValidationResult(bool result, string message)
        {
            Result = result;
            Message = message;
        }
    }
}