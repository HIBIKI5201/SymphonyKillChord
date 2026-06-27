namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///    ステージ結果のミッション項目を表すビューモデル。
    /// </summary>
    public class StageResultMissionItemViewModel
    {
        /// <summary>
        ///   ステージ結果のミッション項目を表すビューモデルを初期化します。
        /// </summary>
        /// <param name="descpription"> ミッションの説明文。 </param>
        /// <param name="isCompleted"> ミッションが達成されたかどうか。 </param>
        public StageResultMissionItemViewModel(
            string descpription,
            bool isCompleted)
        {
            Description = descpription ?? string.Empty;
            IsCompleted = isCompleted;
        }

        /// <summary> ミッションの説明文。 </summary>
        public string Description { get; }

        /// <summary> ミッションが達成されたかどうか。 </summary>
        public bool IsCompleted { get; }
    }
}
