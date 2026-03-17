using DevelopProducts.Design.GameMode.Domain;
using DevelopProducts.Design.GameMode.InfraStructure;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.View
{
    /// <summary>
    ///     敵一体につけるビュークラス。
    ///     EnemyDefinitionとEnemyRuntimeStateを保持し、敵の状態を管理する。
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        public EnemyDefinition EnemyDefinition => _enemyDefinition;
        public EnemyRuntimeState RuntimeState => _runtimeState;

        [SerializeField] private EnemyDefinition _enemyDefinition;
        [SerializeField] private int _maxHp;

        private EnemyRuntimeState _runtimeState;

        private void Awake()
        {
            if (_enemyDefinition == null)
            {
                Debug.LogError("EnemyDefinition is not assigned in EnemyView.");
                return;
            }
            _runtimeState = new EnemyRuntimeState(_maxHp);
        }

        private void Update()
        {
            if (_runtimeState != null && _runtimeState.IsDead)
            {
                Destroy(gameObject);
            }
        }
    }
}
