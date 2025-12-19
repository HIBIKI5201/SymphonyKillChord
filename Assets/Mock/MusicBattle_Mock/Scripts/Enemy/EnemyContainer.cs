using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Character;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     シーン内のエネミー(EnemyManager)を一元管理するコンテナ。
    ///     現在生存している敵をリストで管理し、敵の追加、死亡時の自動削除を担当する。
    /// </summary>
    public class EnemyContainer : ILockOnTargetContainer
    {
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        #region パブリックプロパティ
        /// <summary> すべてのロックオン可能なターゲットの読み取り専用リストを取得します。 </summary>
        public IReadOnlyList<Transform> Targets
        {
            get
            {
                return _enemies
                    .Where(enemy => enemy != null && enemy.gameObject.activeInHierarchy)
                    .Select(enemy => enemy.transform)
                    .ToList();
            }
        }
        /// <summary> 近い順にソートされたロックオン可能なターゲットの読み取り専用リストを取得します。 </summary>
        public IReadOnlyList<Transform> NearerTargets
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
        #endregion

        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     管理中のエネミーをインデックスで取得します。
        ///     インデックスがリスト範囲を超える場合は modulo（循環アクセス）で参照します。
        ///     敵が 0 体の場合は null を返します。
        /// </summary>
        /// <param name="index">取得したいエネミーのインデックス。</param>
        /// <returns>EnemyManagerのインスタンス、またはnull。</returns>
        public EnemyManager this[int index] =>
            0 < _enemies.Count ? _enemies[index % _enemies.Count] : null;

        /// <summary>
        ///     敵をコンテナに登録します。
        ///     生存中リストに追加し、死亡イベントを購読して、死亡時に自動でリストから削除します。
        /// </summary>
        /// <param name="enemy">登録するエネミー。</param>
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
                var nearestEnemy = GetNearestEnemy(_player.Pivot);
                _lockOnManager.ChangeCurrentEnemy(nearestEnemy);
                enemy.gameObject.SetActive(false);
            };
            _deathHandlers[enemy] = handler;
            enemy.OnDeath += handler;
        }

        /// <summary>
        ///     プールからEnemyManagerを取得します。
        /// </summary>
        /// <returns>プール内のEnemyManager、またはnull。</returns>
        public EnemyManager GetFromPool()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : null;
        }

        /// <summary>
        ///     ロックオンマネージャーを設定します。
        /// </summary>
        /// <param name="manager">設定するLockOnManager。</param>
        public void SetLockOnManager(LockOnManager manager) { _lockOnManager = manager; }

        /// <summary>
        ///     プレイヤーキャラクターを設定します。
        /// </summary>
        /// <param name="player">設定するICharacter。</param>
        public void SetCharacter(ICharacter player) => _player = player;

        /// <summary>
        ///     カメラマネージャーを設定します。
        /// </summary>
        /// <param name="camera">設定するCameraManager。</param>
        public void SetCamera(CameraManager camera) => _cameraManager = camera;

        /// <summary>
        ///     プールからEnemyManagerを安全に取得します。
        ///     プールに敵が存在すればdequeueして返し、trueを返します。
        ///     プールが空の場合はenemyにnullをセットして、falseを返します。
        /// </summary>
        /// <param name="enemy">取得したEnemyManagerが格納される。</param>
        /// <returns>プールから取得できた場合は true、取得できなかった場合 false。</returns>
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

        /// <summary>
        ///     プレイヤーに最も近い敵を取得します。
        /// </summary>
        /// <param name="playerPosition">プレイヤーの現在位置。</param>
        /// <returns>最も近い敵のEnemyManager、またはnull。</returns>
        public EnemyManager GetNearestEnemy(Vector3 playerPosition)
        {
            EnemyManager nearestEnemy = _enemies
                .Where(e => e != null && e.gameObject.activeInHierarchy)
                .OrderBy(e => (e.transform.position - playerPosition).sqrMagnitude)
                .FirstOrDefault();

            if (nearestEnemy == null)
            {
                return null;
            }

            return nearestEnemy;
        }
        #endregion

        // PUBLIC_INTERFACE_METHODS (ILockOnTargetContainer のプロパティは#region パブリックプロパティ に移動)
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> カメラマネージャーの参照。 </summary>
        private CameraManager _cameraManager;
        /// <summary> ロックオンマネージャーの参照。 </summary>
        private LockOnManager _lockOnManager;
        /// <summary> プレイヤーキャラクターの参照。 </summary>
        private ICharacter _player;
        /// <summary> 現在アクティブなエネミーのリスト。 </summary>
        private readonly List<EnemyManager> _enemies = new();
        /// <summary> 死亡したエネミーを再利用するためのプール。 </summary>
        private readonly Queue<EnemyManager> _pool = new();
        /// <summary> 敵の死亡イベントハンドラを格納するDictionary。 </summary>
        private readonly Dictionary<EnemyManager, System.Action> _deathHandlers = new();
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
