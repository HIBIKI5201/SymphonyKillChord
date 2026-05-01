namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     画面遷移のルールを表す値オブジェクト。
    /// </summary>
    public readonly struct ScreenTransitionRule
    {
        /// <summary>
        ///     遷移ルールを初期化します。
        /// </summary>
        public ScreenTransitionRule(ScreenTransitionType transitionType, bool keepInHistory)
        {
            TransitionType = transitionType;
            KeepInHistory = keepInHistory;
        }

        /// <summary> 遷移の種類を取得します。 </summary>
        public ScreenTransitionType TransitionType { get; }

        /// <summary> 履歴へ積むかどうかを取得します。 </summary>
        public bool KeepInHistory { get; }
    }
}
