using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     子条件がすべて満たされている場合にミッションがクリアとなる条件グループを表すクラス。
    /// </summary>
    public class AndClearConditionGroup : IMissionClearCondition
    {
        public AndClearConditionGroup(IReadOnlyList<IMissionClearCondition> conditions)
        {
            _conditions = conditions;
        }

        public string GetDescription()
        {
            return "すべての条件を満たす";
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                Debug.LogWarning("クリア条件が設定されていません");
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

        private readonly IReadOnlyList<IMissionClearCondition> _conditions;
    }
}
