using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     子条件がすべて満たされている場合にミッションがクリアとなる条件グループを表すクラス。
    /// </summary>
    public class AndClearConditionGroup : MissionConditionGroup<IMissionClearCondition>, IMissionClearCondition
    {
        /// <summary>
        ///     AndClearConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">クリア条件のリスト。</param>
        public AndClearConditionGroup(IReadOnlyList<IMissionClearCondition> conditions)
            : base(conditions, true, "すべての条件を満たす", "クリア条件が設定されていません。")
        {
        }
    }
}
