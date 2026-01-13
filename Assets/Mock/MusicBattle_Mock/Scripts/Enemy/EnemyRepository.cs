using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミーゲームオブジェクトPrefabを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName =  nameof(EnemyRepository),menuName = "Mock/MusicBattle/Enemy/" + nameof(EnemyRepository), order = 0)]
    public class EnemyRepository : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> エネミーのPrefab。 </summary>
        public GameObject EnemyPrefab => _enemyPrefab;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> エネミーのPrefab。 </summary>
        [SerializeField, Tooltip("エネミーのPrefab。")]
        private GameObject _enemyPrefab;
        #endregion
    }}

