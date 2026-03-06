using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵のスポーン設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySpawnSO", menuName = "Mock/MusicBattle/Enemy/EnemySpawnSO")]
    public class EnemySpawnSO : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> スポーン範囲のX座標。 </summary>
        public float RangeX => _spawnRange.x;
        /// <summary> スポーン範囲のY座標。 </summary>
        public float RangeY => _spawnRange.y;
        /// <summary> スポーン範囲のZ座標。 </summary>
        public float RangeZ => _spawnRange.z;
        /// <summary> 最大同時出現数。 </summary>
        public int MaxEnemyCount => _maxEnemyCount;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> スポーン範囲（Vector3 の各軸に相当）。 </summary>
        [SerializeField, Tooltip("スポーン範囲（Vector3 の各軸に相当）。")]
        private Vector3 _spawnRange = new Vector3(2f, 0f, 2f);

        /// <summary> 最大同時スポーン数。 </summary>
        [SerializeField, Tooltip("最大同時スポーン数。")]
        private int _maxEnemyCount = 3;
        #endregion
    }}

