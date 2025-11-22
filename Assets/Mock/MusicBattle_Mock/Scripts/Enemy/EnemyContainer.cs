using System;
using System.Collections.Generic;
using Mock.MusicBattle.Enemy;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    /// シーン内のエネミー(EnemyManager)を一元管理するコンテナ。
    /// 生存中の敵リストを保持し、敵の追加・死亡時の削除を行う。
    /// </summary>
    public class EnemyContainer : MonoBehaviour
    {
        public EnemyManager this[int index] => 0 < _enemies.Count ? _enemies[index % _enemies.Count] : null;
        
        public void Register(EnemyManager enemy)
        {
            _enemies.Add(enemy);
            enemy.OnDeath += () => _enemies.Remove(enemy);
        }
        private List<EnemyManager> _enemies = new();
    }
}
