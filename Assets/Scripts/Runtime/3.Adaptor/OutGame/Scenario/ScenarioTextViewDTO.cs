namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// 話者名と本文を同一通知で View へ渡す読み取り専用データ。
    /// </summary>
    public readonly ref struct ScenarioTextViewDTO
    {
        /// <summary>
        /// 表示内容を初期化する。
        /// </summary>
        public ScenarioTextViewDTO(string speaker, string message)
        {
            Speaker = speaker ?? string.Empty;
            Message = message ?? string.Empty;
        }

        /// <summary> 話者名を取得する。 </summary>
        public string Speaker { get; }
        /// <summary> 本文を取得する。 </summary>
        public string Message { get; }
    }
}
