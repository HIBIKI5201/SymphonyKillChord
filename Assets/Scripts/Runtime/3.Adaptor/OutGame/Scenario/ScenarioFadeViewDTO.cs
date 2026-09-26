namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// フェード要求を型付きで View へ渡す読み取り専用データ。
    /// </summary>
    public readonly ref struct ScenarioFadeViewDTO
    {
        /// <summary>
        /// フェード要求を初期化する。
        /// </summary>
        public ScenarioFadeViewDTO(
            ScenarioFadeTarget target,
            ScenarioFadeMode mode,
            float start,
            float end,
            float duration)
        {
            Target = target;
            Mode = mode;
            Start = start;
            End = end;
            Duration = duration;
        }

        /// <summary> 対象を取得する。 </summary>
        public ScenarioFadeTarget Target { get; }
        /// <summary> 表示チャネルを取得する。 </summary>
        public ScenarioFadeMode Mode { get; }
        /// <summary> 開始値を取得する。 </summary>
        public float Start { get; }
        /// <summary> 終了値を取得する。 </summary>
        public float End { get; }
        /// <summary> 秒数を取得する。 </summary>
        public float Duration { get; }
    }
}
