using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     子条件がすべて満たされている場合にミッションがクリアとなる条件グループを表すクラス。
    /// </summary>
    public class AndClearConditionGroup : IMissionClearCondition
    {
        /// <summary>
        ///     AndClearConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">クリア条件のリスト。</param>
        public AndClearConditionGroup(IReadOnlyList<IMissionClearCondition> conditions)
        {
            _conditions = conditions;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return "すべての条件を満たす";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                Debug.LogWarning("クリア条件が設定されていません。");
                return false;
            }

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].IsSatisfied(progress))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary> クリア条件のリスト。 </summary>
        private readonly IReadOnlyList<IMissionClearCondition> _conditions;
    }
}
