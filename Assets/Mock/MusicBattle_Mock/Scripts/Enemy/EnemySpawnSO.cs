using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnSO", menuName = "Mock/MusicBattle/Enemy/EnemySpawnSO")]
    public class EnemySpawnSO : ScriptableObject
    {
        /// <summary> スポーン範囲 X </summary>
        public float XRange => _xRange;

        /// <summary> スポーン範囲 Y </summary>
        public float YRange => _yRange;

        /// <summary> スポーン範囲 Z </summary>
        public float ZRange => _zRange;

        /// <summary> 最大出現数 </summary>
        public int MaxEnemyCount => _maxEnemyCount;

        [Header("スポーン範囲（Vector3 の各軸に相当）")]
        [SerializeField] private float _xRange;
        [SerializeField] private float _yRange;
        [SerializeField] private float _zRange;

        [Header("最大同時スポーン数")]
        [SerializeField] private int _maxEnemyCount = 3;
    }
}
