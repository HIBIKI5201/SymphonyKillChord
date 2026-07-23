using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     目標シーケンスの1ステップを表すクラス。達成条件と、ステップ開始時に案内するメッセージを保持する。
    /// </summary>
    public sealed class ObjectiveSequenceStep
    {
        /// <summary>
        ///     ObjectiveSequenceStep クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="condition"> このステップの達成条件です。 </param>
        /// <param name="guideMessageText"> ステップ開始時に案内するメッセージです。不要な場合は空文字またはnullです。 </param>
        public ObjectiveSequenceStep(IMissionClearCondition condition, string guideMessageText)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            GuideMessageText = guideMessageText;
        }

        /// <summary> このステップの達成条件です。 </summary>
        public IMissionClearCondition Condition { get; }

        /// <summary> ステップ開始時に案内するメッセージです。未設定の場合はnullまたは空文字です。 </summary>
        public string GuideMessageText { get; }
    }
}
