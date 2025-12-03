using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnSO", menuName = "Mock/MusicBattle/Enemy/EnemySpawnSO")]
    public class EnemySpawnSO : ScriptableObject
    {
        /// <summary> スポーン範囲 X </summary>
        public float XRange => _spawnRange.x;

        /// <summary> スポーン範囲 Y </summary>
        public float YRange => _spawnRange.y;

        /// <summary> スポーン範囲 Z </summary>
        public float ZRange => _spawnRange.z;

        /// <summary> 最大出現数 </summary>
        public int MaxEnemyCount => _maxEnemyCount;

        [Header("スポーン範囲（Vector3 の各軸に相当）")]
        [SerializeField] private Vector3 _spawnRange = new Vector3(2f, 0f, 2f);

        [Header("最大同時スポーン数")]
        [SerializeField] private int _maxEnemyCount = 3;
    }
}
