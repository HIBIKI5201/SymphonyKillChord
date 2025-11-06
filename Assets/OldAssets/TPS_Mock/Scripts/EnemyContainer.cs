using UnityEngine;

namespace Mock.TPS
{
    public class EnemyContainer : MonoBehaviour
    {
        private EnemyManager[] _enemies;

        private void Awake()
        {
            _enemies = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        }
    }
}
