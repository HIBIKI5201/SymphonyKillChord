using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミーゲームオブジェクトPrefabを保持する。
    /// </summary>
    [CreateAssetMenu(fileName =  nameof(EnemyRepository),menuName = "Mock/MusicBattle/Enemy/" + nameof(EnemyRepository), order = 0)]
    public class EnemyRepository : ScriptableObject
    {
        public GameObject EnemyPrefab => _enemyPrefab;
        [SerializeField] private GameObject _enemyPrefab;
    }
}
