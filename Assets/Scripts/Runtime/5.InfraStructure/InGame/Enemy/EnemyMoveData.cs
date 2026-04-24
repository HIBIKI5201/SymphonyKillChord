using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     敵移動のデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnemyMoveData), menuName = "KillChord/Enemy" + nameof(EnemyMoveData))]
    public class EnemyMoveData : ScriptableObject
    {
        public float MoveSpeed => _moveSpeed;
        public float AttackRangeMin => _attackRangeMin;
        public float AttackRangeMax => _attackRangeMax;

        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _attackRangeMin;
        [SerializeField] private float _attackRangeMax;
    }
}
