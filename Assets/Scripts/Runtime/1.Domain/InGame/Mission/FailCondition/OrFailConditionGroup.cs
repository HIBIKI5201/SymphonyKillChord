using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     いずれかの子条件が満たされている場合にミッションが失敗となる条件グループを表すクラス。
    /// </summary>
    public class OrFailConditionGroup : MissionConditionGroup<IMissionFailCondition>, IMissionFailCondition
    {
        /// <summary>
        ///     OrFailConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">失敗条件のリスト。</param>
        public OrFailConditionGroup(IReadOnlyList<IMissionFailCondition> conditions)
            : base(conditions, false, "いずれかの条件を満たす。", "失敗条件が設定されていません。")
        {
        }
    }
}
