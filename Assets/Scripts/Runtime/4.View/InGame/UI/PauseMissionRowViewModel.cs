namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ画面のミッション欄に出す、ミッション1件分の表示データ。
    /// </summary>
    public sealed class PauseMissionRowViewModel
    {
        /// <summary>
        ///     ミッションの表示文と達成状況を指定して生成する。
        /// </summary>
        /// <param name="description"> ミッションの表示文。 </param>
        /// <param name="isAchieved"> 達成している場合はtrue。 </param>
        /// <param name="statusText"> 達成状況の表示文。 </param>
        public PauseMissionRowViewModel(string description, bool isAchieved, string statusText)
        {
            Description = description;
            IsAchieved = isAchieved;
            StatusText = statusText;
        }

        /// <summary> ミッションの表示文。 </summary>
        public string Description { get; }

        /// <summary> 達成している場合はtrue。 </summary>
        public bool IsAchieved { get; }

        /// <summary> 達成状況の表示文。 </summary>
        public string StatusText { get; }
    }
}
