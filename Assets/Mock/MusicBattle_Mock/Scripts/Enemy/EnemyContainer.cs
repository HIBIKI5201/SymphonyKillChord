using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Character;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     シーン内のエネミー(EnemyManager)を一元管理するコンテナ。
    ///     現在生存している敵をリストで管理。
    ///     敵の追加、死亡時の自動削除を担当する。
    /// </summary>
    public class EnemyContainer : ILockOnTargetContainer
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

        public IReadOnlyList<Transform> Targets
        {
            get
            {
                return _enemies
                    .Where(e => e != null && e.gameObject.activeInHierarchy)
                    .OrderBy(e => Vector3.SqrMagnitude(e.transform.position - _player.Pivot))
                    .Select(e => e.transform)
                    .ToList();
            }
        }


        /// <summary>
        ///     敵をコンテナに登録する。
        ///     生存中リストに追加。
        ///     死亡イベントを購読し、死亡時に自動でリストから削除する。
        /// </summary>
        /// <param name="enemy"> 登録するエネミー。 </param>
        public void Register(EnemyManager enemy)
        {
            if (_enemies.Contains(enemy)) return;
            _enemies.Add(enemy);
            enemy.gameObject.SetActive(true);
            if (_deathHandlers.TryGetValue(enemy, out var oldHandler))
            {
                enemy.OnDeath -= oldHandler;
            }
            System.Action handler = () =>
            {
                _enemies.Remove(enemy);
                _pool.Enqueue(enemy);
                var (nearestEnemy, nearestTransform) = GetNearestEnemy(_player.Pivot);
                _lockOnManager.ChangeCurrentEnemy(nearestEnemy);
                _cameraManager.HandleLockOn(nearestTransform);

                enemy.gameObject.SetActive(false);
            };
            _deathHandlers[enemy] = handler;
            enemy.OnDeath += handler;
        }

        public EnemyManager GetFromPool()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : null;
        }
        //  　参照をもらう方法でほかに方法ないかな　現在ゲームマネージャーから受け取っている。
        public void SetLockOnManager(LockOnManager manager) { _lockOnManager = manager; }
        public void SetCharacter(ICharacter player) => _player = player;
        public void SetCamera(CameraManager camera) => _cameraManager = camera;
        /// <summary>
        ///     プールからEnemyManagerを安全に取得する。
        ///     プールに敵が存在すればdequeueして返し、trueを返す。
        ///     プールが空の場合はenemyにnullをセットして、falseを返す。
        /// </summary>
        /// <param name="enemy"> 取得したEnemyManagerが格納される </param>
        /// <returns> プールから取得できた場合は true、取得でいなかった場合 false </returns>
        public bool TryGetFromPool(out EnemyManager enemy)
        {
            if (_pool.Count > 0)
            {
                enemy = _pool.Dequeue();
                return true;
            }
            else
            {
                enemy = null;
                return false;
            }
        }
        public (EnemyManager enemy, Transform target) GetNearestEnemy(Vector3 playerPosition)
        {
            EnemyManager nearestEnemy = _enemies
                .Where(e => e != null && e.gameObject.activeInHierarchy)
                .OrderBy(e => (e.transform.position - playerPosition).sqrMagnitude)
                .FirstOrDefault();

            if (nearestEnemy == null)
            {
                return (null, null);
            }

            return (nearestEnemy, nearestEnemy.transform);
        }
        
        
        private CameraManager _cameraManager;
        private LockOnManager _lockOnManager;
        private ICharacter _player;
        private List<EnemyManager> _enemies = new();
        private Queue<EnemyManager> _pool = new();
        private Dictionary<EnemyManager, System.Action> _deathHandlers = new();


    }
}
