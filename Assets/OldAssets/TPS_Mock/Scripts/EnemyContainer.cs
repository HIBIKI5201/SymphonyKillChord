using System.Collections.Generic;
using UnityEngine;

namespace Mock.TPS
{
    public class EnemyContainer : MonoBehaviour
    {
        public EnemyManager this[int index] => 0 < _enemies.Count ? _enemies[index % _enemies.Count] : null;

        private List<EnemyManager> _enemies = new();

        private void Awake()
        {
            foreach (EnemyManager e in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
            {
                Register(e);
            }
        }

        private void Register(EnemyManager enemy)
        {
            _enemies.Add(enemy);
            enemy.OnDeath += () => _enemies.Remove(enemy);
        }
    }
}
