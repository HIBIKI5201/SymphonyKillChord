using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     敵撃破数を記録するEntityクラス。
    /// </summary>
    public class EnemyKillRecord
    {
        /// <summary> 合計撃破数を取得します。 </summary>
        public int TotalKillCount => _totalKillCount;

        /// <summary>
        ///     敵撃破数を記録します。すでに記録されている敵の場合はカウントを増やします。
        /// </summary>
        /// <param name="enemyKey">敵のキー。</param>
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
        ///     指定した敵の撃破数を取得します。
        ///     記録されていない敵の場合は0を返します。
        /// </summary>
        /// <param name="enemyKey">敵のキー。</param>
        /// <returns>撃破数。</returns>
        public int GetKillCount(EnemyMissionKey enemyKey)
        {
            return _killCounts.TryGetValue(enemyKey, out var count) ? count : 0;
        }

        /// <summary> 敵ごとの撃破数。 </summary>
        private readonly Dictionary<EnemyMissionKey, int> _killCounts = new();
        /// <summary> 合計撃破数。 </summary>
        private int _totalKillCount;
    }
}
