using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    public class OrFailConditionGroup : IMissionFailCondition
    {
        public OrFailConditionGroup(IReadOnlyList<IMissionFailCondition> conditions)
        {
            _conditions = conditions;
        }

        public string GetDescription()
        {
            return "いずれかの条件を満たす";
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
                if (_conditions[i].IsSatisfied(progress))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly IReadOnlyList<IMissionFailCondition> _conditions;
    }
}
