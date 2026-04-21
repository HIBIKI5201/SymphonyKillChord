using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     指定した敵を一定数以上倒すとクリアとなる条件。
    /// </summary>
    public class EnemyKillCountClearCondition : IMissionClearCondition
    {
        public EnemyKillCountClearCondition(EnemyMissionKey key, int requiredKillCount, string desplayName)
        {
            if (requiredKillCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredKillCount),
                    requiredKillCount,
                    "必要撃破数は1以上である必要があります。");
            }

            _key = key;
            _requiredKillCount = requiredKillCount;
            _desplayName = desplayName;
        }

        public string GetDescription()
        {
            return $"{_desplayName}を{_requiredKillCount}体以上倒す";
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.EnemyKillRecord.GetKillCount(_key) >= _requiredKillCount;
        }

        private readonly EnemyMissionKey _key;
        private readonly int _requiredKillCount;
        private readonly string _desplayName;
    }
}
