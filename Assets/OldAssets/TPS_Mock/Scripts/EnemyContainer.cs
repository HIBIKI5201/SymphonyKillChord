using UnityEngine;

namespace Mock.TPS
{
    public class EnemyContainer : MonoBehaviour
    {
        public EnemyManager this[int index] => _enemies[index % _enemies.Length];

        private EnemyManager[] _enemies;

        private void Awake()
        {
            _enemies = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        }
    }
}
