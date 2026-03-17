using DevelopProducts.Design.GameMode.InfraStructure;
using System;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     敵の撃破数に関するクリア条件。
    ///     例えば、特定の敵を一定数撃破することなど、敵の撃破数に関連する条件を管理するためのクラス。
    /// </summary>
    [Serializable]
    public class EnemyKillCountClearCondition : IClearCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            return context.EnemyKillStatics.GetKillCount(_enemyDefinition) >= _requiredCount;
        }

        public string GetDescription()
        {
            string enemyName = _enemyDefinition != null ? _enemyDefinition.DisplayName : "Unknown Enemy";
            return $"{enemyName}を{_requiredCount}体以上撃破する";
        }

        [SerializeField] private EnemyDefinition _enemyDefinition;
        [SerializeField] private int _requiredCount;
    }
}
