using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnSO", menuName = "Mock/MusicBattle/Enemy/EnemySpawnSO")]
    public class EnemySpawnSO : ScriptableObject
    {
        /// <summary> スポーン範囲 X </summary>
        public float XVector3 => xVector3;

        /// <summary> スポーン範囲 Y </summary>
        public float YVector3 => yVector3;

        /// <summary> スポーン範囲 Z </summary>
        public float ZVector3 => zVector3;

        /// <summary> 最大出現数 </summary>
        public int MaxEnemyCount => maxEnemyCount;

        [Header("スポーン範囲（Vector3 の各軸に相当）")]
        [SerializeField] private float xVector3;
        [SerializeField] private float yVector3;
        [SerializeField] private float zVector3;

        [Header("最大同時スポーン数")]
        [SerializeField] private int maxEnemyCount = 3;
    }
}
