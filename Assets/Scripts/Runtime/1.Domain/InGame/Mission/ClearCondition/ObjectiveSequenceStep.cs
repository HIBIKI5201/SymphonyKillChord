using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     目標シーケンスの1ステップを表すクラス。達成条件、ステップ開始時に案内するメッセージ、
    ///     およびステップ進入時アクションを保持する。
    /// </summary>
    public sealed class ObjectiveSequenceStep
    {
        /// <summary>
        ///     ObjectiveSequenceStep クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="condition"> このステップの達成条件です。 </param>
        /// <param name="guideMessageText"> ステップ開始時に案内するメッセージです。不要な場合は空文字またはnullです。 </param>
        /// <param name="entryActions">ステップ進入時に実行するアクション一覧です。不要な場合はnullまたは空の一覧です。</param>
        public ObjectiveSequenceStep(
            IMissionClearCondition condition,
            string guideMessageText,
            IReadOnlyList<IMissionStepEntryAction> entryActions)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            GuideMessageText = guideMessageText;
            EntryActions = CreateEntryActions(entryActions);
        }

        /// <summary> このステップの達成条件です。 </summary>
        public IMissionClearCondition Condition { get; }

        /// <summary> ステップ開始時に案内するメッセージです。未設定の場合はnullまたは空文字です。 </summary>
        public string GuideMessageText { get; }

        /// <summary> ステップ進入時に実行するアクション一覧です。 </summary>
        public IReadOnlyList<IMissionStepEntryAction> EntryActions { get; }

        /// <summary>ステップ開始時の進行値を条件へ通知します。</summary>
        /// <param name="progress">Mission進行状況です。</param>
        public void Begin(MissionProgress progress)
        {
            if (Condition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }

        /// <summary>
        ///     ステップ進入時アクション一覧を生成します。
        /// </summary>
        /// <param name="entryActions">元となるアクション一覧です。</param>
        /// <returns>変更できないアクション一覧です。</returns>
        private static IReadOnlyList<IMissionStepEntryAction> CreateEntryActions(
            IReadOnlyList<IMissionStepEntryAction> entryActions)
        {
            if (entryActions == null || entryActions.Count == 0)
            {
                return Array.Empty<IMissionStepEntryAction>();
            }

            List<IMissionStepEntryAction> actions = new(entryActions.Count);
            for (int i = 0; i < entryActions.Count; i++)
            {
                IMissionStepEntryAction entryAction = entryActions[i];
                if (entryAction == null)
                {
                    throw new ArgumentException(
                        $"{nameof(entryActions)}[{i}] must not be null.",
                        nameof(entryActions));
                }

                actions.Add(entryAction);
            }

            return actions.AsReadOnly();
        }
    }
}
