using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class ElapsedTimeFailConditionAsset : MissionFailConditionAssetBase
    {
        public override IMissionFailCondition Create()
        {
            return new ElapsedTimeFailCondition(_requiredElapsedTime);
        }

        protected override string BuildSummary()
        {
            return $"{_requiredElapsedTime}秒を超えたら失敗する条件";
        }

        [SerializeField] private float _requiredElapsedTime;
    }
}
