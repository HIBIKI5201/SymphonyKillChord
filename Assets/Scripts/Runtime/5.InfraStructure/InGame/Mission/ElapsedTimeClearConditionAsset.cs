using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class ElapsedTimeClearConditionAsset : MissionClearConditionAssetBase
    {
        public override IMissionClearCondition Create()
        {
            return new ElapsedTimeClearCondition(_requiredElapsedTime);
        }

        protected override string BuildSummary()
        {
            return $"{_requiredElapsedTime}秒生存する条件";
        }

        [SerializeField] private float _requiredElapsedTime;
    }
}
