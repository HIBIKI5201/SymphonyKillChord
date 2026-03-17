using DevelopProducts.Design.GameMode.InfraStructure;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     敵の撃破数を管理するクラス。
    ///     敵の種類ごとに撃破数を記録し、必要に応じて取得できるようにする。
    /// </summary>
    public class EnemyKillStatics
    {
        /// <summary>
        ///     敵を撃破した際に呼び出されるメソッド。敵の定義を受け取り、その敵の撃破数を更新する。
        /// </summary>
        /// <param name="enemyDefinition"></param>
        public void RecordKill(EnemyDefinition enemyDefinition)
        {
            if (enemyDefinition == null || string.IsNullOrEmpty(enemyDefinition.Id))
            {
                Debug.LogWarning("Invalid enemy definition provided to RecordKill.");
                return;
            }

            if (_enemyKillCounts.ContainsKey(enemyDefinition.Id))
            {
                _enemyKillCounts[enemyDefinition.Id]++;
            }
            else
            {
                _enemyKillCounts[enemyDefinition.Id] = 1;
            }
        }

        /// <summary>
        ///     敵のIDを指定して、その敵の撃破数を取得するメソッド。指定された敵が撃破されていない場合は0を返す。
        /// </summary>
        /// <param name="enemyId"></param>
        /// <returns></returns>
        public int GetKillCount(EnemyDefinition enemyDef)
        {
            if (enemyDef == null || string.IsNullOrEmpty(enemyDef.Id))
            {
                Debug.LogWarning("Invalid enemy ID provided to GetKillCount.");
                return 0;
            }
            return _enemyKillCounts.TryGetValue(enemyDef.Id, out int count) ? count : 0;
        }

        private readonly Dictionary<string, int> _enemyKillCounts = new();
    }
}
