using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class PlayerDeadFailConditionAsset : MissionFailConditionAssetBase
    {
        public override IMissionFailCondition Create()
        {
            return new PlayerDeadFailCondition();
        }

        protected override string BuildSummary()
        {
            return $"プレイヤーが死んだら失敗する条件";
        }
    }
}
