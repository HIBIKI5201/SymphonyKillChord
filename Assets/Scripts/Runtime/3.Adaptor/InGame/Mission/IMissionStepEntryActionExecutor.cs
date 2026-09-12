using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     特定種類のミッション目標ステップ進入時アクションを実行するインターフェースです。
    /// </summary>
    public interface IMissionStepEntryActionExecutor
    {
        /// <summary> 実行対象となる目標ステップ進入時アクションの型です。 </summary>
        Type EntryActionType { get; }

        /// <summary>
        ///     目標ステップ進入時アクションを実行します。
        /// </summary>
        /// <param name="entryAction">実行するアクション</param>
        void Execute(IMissionStepEntryAction entryAction);
    }
}
