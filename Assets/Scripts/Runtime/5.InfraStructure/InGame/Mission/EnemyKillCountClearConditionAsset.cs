using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class EnemyKillCountClearConditionAsset : MissionClearConditionAssetBase
    {
        public override IMissionClearCondition Create()
        {
            return new EnemyKillCountClearCondition(
                _enemyMissionKeyAsset.Id,
                _requiredKillCount,
                _enemyMissionKeyAsset.DesplayName);
        }

        protected override string BuildSummary()
        {
            string enemyName = _enemyMissionKeyAsset != null ? _enemyMissionKeyAsset.DesplayName : "null";
            return $"{enemyName}を{_requiredKillCount}体以上倒す条件";
        }

        [SerializeField] private EnemyMissionKeyAsset _enemyMissionKeyAsset;
        [SerializeField] private int _requiredKillCount;
    }
}
