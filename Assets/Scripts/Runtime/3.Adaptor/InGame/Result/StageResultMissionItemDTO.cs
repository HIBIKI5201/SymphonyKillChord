namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///    ステージリザルトのミッションアイテムを表すDTO。
    /// </summary>
    public readonly struct StageResultMissionItemDTO
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="description"> 表示文。 </param>
        /// <param name="isCompleted"> 完了状態。 </param>
        public StageResultMissionItemDTO(string description, bool isCompleted)
        {
            Description = description;
            IsCompleted = isCompleted;
        }

        /// <summary> 表示文。 </summary>
        public string Description { get; }

        /// <summary> 達成済みか。 </summary>
        public bool IsCompleted { get; }
    }
}
