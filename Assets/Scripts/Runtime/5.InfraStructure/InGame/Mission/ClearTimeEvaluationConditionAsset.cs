using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class ClearTimeEvaluationConditionAsset : MissionEvaluationConditionAssetBase
    {
        public override IMissionEvaluationCondition Create()
        {
            return new ClearTimeEvaluationCondition(_clearTime);
        }

        protected override string BuildSummary()
        {
            return $"クリアタイムが{_clearTime}秒以下である条件";
        }

        [SerializeField] private float _clearTime;
    }
}
