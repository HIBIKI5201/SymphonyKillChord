using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>Objectiveステップ開始時の進行値を基準として評価する条件です。</summary>
    public interface IObjectiveSequenceStepCondition
    {
        /// <summary>ステップ開始時の進行値を記録します。</summary>
        /// <param name="progress">Mission進行状況です。</param>
        void BeginStep(MissionProgress progress);
    }

    /// <summary>
    ///     目標シーケンスの1ステップを表すクラス。達成条件と、ステップ開始時に案内するメッセージを保持する。
    ///     Wave開始やポップアップ表示などの特殊な振る舞いは、Step自体ではなく<see cref="Condition"/>側(デコレータ条件)が表現する。
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

        /// <summary>ステップ開始時の進行値を条件へ通知します。</summary>
        /// <param name="progress">Mission進行状況です。</param>
        public void Begin(MissionProgress progress)
        {
            if (Condition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }
    }
}
