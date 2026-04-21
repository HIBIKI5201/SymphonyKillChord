using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     敵撃破数を記録するEntityクラス。
    /// </summary>
    public class EnemyKillRecord
    {
        public int TotalKillCount => _totalKillCount;

        /// <summary>
        ///     敵撃破数を記録する。すでに記録されている敵の場合はカウントを増やす。
        /// </summary>
        /// <param name="enemyKey"></param>
        public void RecordKill(EnemyMissionKey enemyKey)
        {
            if (_killCounts.ContainsKey(enemyKey))
            {
                _killCounts[enemyKey]++;
            }
            else
            {
                _killCounts[enemyKey] = 1;
            }

            _totalKillCount++;
        }

        /// <summary>
        ///     指定した敵の撃破数を取得する。
        ///     記録されていない敵の場合は0を返す。
        /// </summary>
        /// <param name="enemyKey"></param>
        /// <returns></returns>
        public int GetKillCount(EnemyMissionKey enemyKey)
        {
            return _killCounts.TryGetValue(enemyKey, out var count) ? count : 0;
        }

        private readonly Dictionary<EnemyMissionKey, int> _killCounts = new();
        private int _totalKillCount;
    }
}
