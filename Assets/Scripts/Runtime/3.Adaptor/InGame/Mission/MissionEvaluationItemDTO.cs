namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     評価ミッション1件分の情報を保持するDTO構造体。
    /// </summary>
    public readonly struct MissionEvaluationItemDTO
    {
        /// <summary>
        ///     MissionEvaluationItemDTO 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="description"> 表示文。 </param>
        /// <param name="displayState"> HUD表示状態。 </param>
        public MissionEvaluationItemDTO(string description, MissionEvaluationDisplayState displayState)
        {
            Description = description;
            DisplayState = displayState;
        }

        /// <summary> 表示文を取得します。 </summary>
        public string Description { get; }

        /// <summary> HUD表示状態を取得します。 </summary>
        public MissionEvaluationDisplayState DisplayState { get; }
    }
}
