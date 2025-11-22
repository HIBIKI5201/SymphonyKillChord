using System;
using System.Collections.Generic;
using Mock.MusicBattle.Enemy;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     シーン内のエネミー(EnemyManager)を一元管理するコンテナ。
    ///     現在生存している敵をリストで管理。
    ///     敵の追加、死亡時の自動削除を担当する。
    /// </summary>
    public class EnemyContainer
    {
        /// <summary>
        ///     管理中のエネミーをインデックスで取得する。
        ///     インデックスがリスト範囲を超える場合は modulo（循環アクセス）で参照する。
        ///     敵が 0 体の場合は null を返す。
        /// </summary>
        /// <param name="index"> 取得したいエネミーのインデックス。 </param>
        /// <returns> EnemyManager または null。 </returns>
        public EnemyManager this[int index] =>
            0 < _enemies.Count ? _enemies[index % _enemies.Count] : null;

        /// <summary>
        ///     敵をコンテナに登録する。
        ///     生存中リストに追加。
        ///     死亡イベントを購読し、死亡時に自動でリストから削除する。
        /// </summary>
        /// <param name="enemy"> 登録するエネミー。 </param>
        public void Register(EnemyManager enemy)
        {
            _enemies.Add(enemy);
            enemy.OnDeath += () =>
            {
                _enemies.Remove(enemy);
                _pool.Enqueue(enemy);
                enemy.gameObject.SetActive(false);
            };
            
        }

        public EnemyManager GetFromPool()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : null;
        }
       
        private List<EnemyManager> _enemies = new();
        private Queue<EnemyManager> _pool = new();
        
    }
}