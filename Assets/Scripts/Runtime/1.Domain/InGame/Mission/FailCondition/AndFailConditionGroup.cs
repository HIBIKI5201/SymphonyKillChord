using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     すべての子条件が満たされている場合にミッションが失敗となる条件グループを表すクラス。
    /// </summary>
    public class AndFailConditionGroup : MissionConditionGroup<IMissionFailCondition>, IMissionFailCondition
    {
        /// <summary>
        ///     AndFailConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">失敗条件のリスト。</param>
        public AndFailConditionGroup(IReadOnlyList<IMissionFailCondition> conditions)
            : base(conditions, true, "すべての条件を満たす。", "失敗条件が設定されていません。")
        {
        }
    }
}
