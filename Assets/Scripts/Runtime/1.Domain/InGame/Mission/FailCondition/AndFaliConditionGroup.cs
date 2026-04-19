using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     すべての子条件が満たされている場合にミッションが失敗となる条件グループを表すクラス。
    /// </summary>
    public class AndFaliConditionGroup : IMissionFailCondition
    {
        public AndFaliConditionGroup(IReadOnlyList<IMissionFailCondition> conditions)
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

        private readonly IReadOnlyList<IMissionFailCondition> _conditions;
    }
}
