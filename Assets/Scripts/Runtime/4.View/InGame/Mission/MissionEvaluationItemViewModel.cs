using KillChord.Runtime.Adaptor.InGame.Mission;

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
        /// <param name="displayState">表示状態。</param>
        public MissionEvaluationItemViewModel(string description, MissionEvaluationDisplayState displayState)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new System.ArgumentException("Description cannot be null or empty.", nameof(description));
            }

            Description = description;
            DisplayState = displayState;
        }

        /// <summary> 表示文を取得します。 </summary>
        public string Description { get; }

        /// <summary> 表示状態を取得します。 </summary>
        public MissionEvaluationDisplayState DisplayState { get; }
    }
}
