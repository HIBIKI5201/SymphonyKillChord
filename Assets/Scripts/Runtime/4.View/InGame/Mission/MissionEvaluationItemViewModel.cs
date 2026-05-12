namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     評価ミッション1件分の情報を保持するViewModelクラス。
    /// </summary>
    public class MissionEvaluationItemViewModel
    {
        /// <summary>
        ///     MissionEvaluationItemViewModel クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="description">説明文。</param>
        /// <param name="isAchieved">達成したかどうか。</param>
        public MissionEvaluationItemViewModel(string description, bool isAchieved)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new System.ArgumentException("Description cannot be null or empty.", nameof(description));
            }

            Description = description;
            IsAchieved = isAchieved;
        }

        /// <summary> 表示文を取得します。 </summary>
        public string Description { get; }

        /// <summary> 達成済みかを取得します。 </summary>
        public bool IsAchieved { get; }
    }
}
