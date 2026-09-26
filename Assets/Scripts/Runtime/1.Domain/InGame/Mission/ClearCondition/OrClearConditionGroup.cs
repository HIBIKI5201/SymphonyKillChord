using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     いずれかの子条件が満たされている場合にミッションがクリアとなる条件グループを表すクラス。
    /// </summary>
    public class OrClearConditionGroup : MissionConditionGroup<IMissionClearCondition>, IMissionClearCondition
    {
        /// <summary>
        ///     OrClearConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">クリア条件のリスト。</param>
        public OrClearConditionGroup(IReadOnlyList<IMissionClearCondition> conditions)
            : base(conditions, false, "いずれかの条件を満たす。", "クリア条件が設定されていません。")
        {
        }
    }
}
