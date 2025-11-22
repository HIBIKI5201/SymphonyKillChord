using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミー(EnemyManager)生成を一元管理するファクトリ。
    ///     プールを用いてエネミーの生成/再利用を行う。
    ///     死亡時にプールへ戻す処理もセットアップする。
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        /// <summary>
        ///     Factory が利用するコンテナ・ターゲット・プレファブを初期化する。
        ///     Factory 使用前に必ず呼び出す必要がある。
        /// </summary>
        /// <param name="enemyContainer"> 生成した敵を登録するコンテナ。 </param>
        /// <param name="target"> 敵が追従・攻撃する対象。 </param>
        /// <param name="enemy"> 生成元となるエネミーのプレファブ。 </param>
        public void Init(EnemyContainer enemyContainer, Transform target, GameObject enemy)
        {
            _enemyContainer = enemyContainer;
            _target = target;
            _enemyPrefab = enemy;
        }

        /// <summary>
        ///     エネミーを生成、またはプールから再利用して返す。
        ///     プールに残っていれば再利用
        ///     なければ Instantiate
        ///     死亡時にプールへ戻すコールバックも設定
        /// </summary>
        /// <param name="status"> エネミーのステータス。 </param>
        /// <param name="position"> 生成位置 。</param>
        /// <returns> 生成または再利用された EnemyManager 。</returns>
        public EnemyManager Spawn(EnemyStatus status, Vector3 position)
        {
            var enemy = _enemyContainer.GetFromPool();

            if (enemy == null)
            {
                enemy = Instantiate(_enemyPrefab).GetComponent<EnemyManager>();
            }
            
            enemy.SetTarget(_target);
            enemy.InitializeMover();
            enemy.transform.position = position;
            
            _enemyContainer.Register(enemy);
            enemy.gameObject.SetActive(true);
            return enemy;
        }

        [SerializeField, Tooltip("エネミーのプレファブ")]
        private GameObject _enemyPrefab;
        private EnemyContainer _enemyContainer;
        private Transform _target;
    }
}
