using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵の主要な処理を一括して管理するクラス。
    ///     HP管理、ターゲット設定、移動ロジック初期化、死亡イベントの通知など、
    ///     敵個体に関する中核処理を提供する。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyManager : MonoBehaviour, ICharacter
    {
        #region Publicイベント
        /// <summary>
        ///     ヘルスが0になったときに発火するイベント。
        ///     オブジェクトプールへ戻す処理や、コンテナから削除する処理などがここに紐づけられる。
        /// </summary>
        public event Action OnDeath
        {
            add => _healthEntity.OnDeath += value;
            remove => _healthEntity.OnDeath -= value;
        }

        /// <summary> 敵が攻撃を行ったときに発火するイベント。 </summary>
        public event Action OnAttack
        {
            add => _enemyMover.OnAttack += value;
            remove => _enemyMover.OnAttack -= value;
        }

        /// <summary> 敵が射程外に出たときに発火するイベント。 </summary>
        public event Action OnOutOfRange
        {
            add => _enemyMover.OnOutOfRange += value;
            remove => _enemyMover.OnOutOfRange -= value;
        }
        #endregion

        #region パブリックプロパティ
        /// <summary> 敵自身のTransform（ロックオン時などに参照される）。 </summary>
        public Transform LockTarget => _lockTarget;
        /// <summary> 敵のピボット位置を取得します。 </summary>
        public Vector3 Pivot => _pivotTransform.position;
        /// <summary> 敵のHealthEntityを取得します。 </summary>
        public HealthEntity HealthEntity => _healthEntity;
        /// <summary> 敵がロックオンされているかどうか。 </summary>
        public bool IsLockOn => _isLockOn;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     敵の初期化をまとめた関数。
        /// </summary>
        /// <param name="target">ターゲットのTransform。</param>
        /// <param name="musicMg">音楽同期マネージャー。</param>
        /// <param name="position">敵の初期位置。</param>
        public void Init(Transform target, MusicSyncManager musicMg, Vector3 position)
        {
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
            SetTarget(target);
            InitializeMover();
            InitMusic(musicMg);
            _attackIndicater = new(_attackDecal);
            transform.position = position;
        }

        /// <summary>
        ///     この敵が追従・攻撃の基準とするターゲットを設定します。
        /// </summary>
        /// <param name="target">追従対象のTransform。</param>
        public void SetTarget(Transform target)
        {
            _target = target;
            _player = target.GetComponent<ICharacter>();
        }

        /// <summary>
        ///     Rigidbody やロックオン用 Transform などの初期化を行い、初期のヘルスを設定します。
        /// </summary>
        /// <param name="lockon">ロックオン対象のTransform。</param>
        public void SetLockOn(Transform lockon)
        {
            if (lockon == null || lockon.gameObject == null)
                return;
            _isLockOn = _lockTarget == lockon;
            Debug.Log($"{gameObject.name} ロックオン対象: {(_isLockOn ? "ロックオン中" : "ロックオン解除")}です。");
        }

        /// <summary>
        ///     音楽関係を使うクラスを初期化します。
        /// </summary>
        /// <param name="music">音楽同期マネージャー。</param>
        public void InitMusic(MusicSyncManager music)
        {
            _musicSyncManager = music;
            _enemyAttack = new EnemyAttack(this, _musicSyncManager,
                _encountSo, _battaleSo, _player, _enemyStatus);
        }

        /// <summary>
        ///     エネミーの移動処理を担当する EnemyMover を初期化します。
        ///     必要なデータ（ステータス、ターゲット、Rigidbody）が揃っていない場合は初期化を中断します。
        /// </summary>
        public void InitializeMover()
        {
            if (_enemyStatus == null || _target == null)
            {
                Debug.Log("EnemyStatusまたはTargetがnullです。");
                return;
            }

            _enemyMover = new EnemyMover(_target, _lockTarget, _enemyStatus, _agent, this);
        }

        /// <summary>
        ///     エネミーにダメージを与え、HP を減らします。
        ///     HP が 0 になった場合は OnDeath が発火します。
        /// </summary>
        /// <param name="damage">与えるダメージ量。</param>
        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);
        #endregion

        #region インスペクター表示フィールド
        /// <summary> エネミーのステータス。 </summary>
        [SerializeField, Tooltip("エネミーのステータス。")]
        private EnemyStatus _enemyStatus;
        /// <summary> エネミーの戦闘時の音楽情報。 </summary>
        [SerializeField, Tooltip("エネミーの戦闘時の音楽情報。")]
        private EnemyMusicSO _battaleSo;
        /// <summary> エネミーの接敵時の音楽情報。 </summary>
        [SerializeField, Tooltip("エネミーの接敵時の音楽情報。")]
        private EnemyMusicSO _encountSo;
        /// <summary> エネミーのピボット位置。 </summary>
        [SerializeField, Tooltip("エネミーのピボット位置。")]
        private Transform _pivotTransform;
        [SerializeField, Tooltip("敵の攻撃インジケーター")]
        private DecalProjector _attackDecal;
        #endregion

        #region プライベートフィールド
        /// <summary> プレイヤーのICharacter参照。 </summary>
        private ICharacter _player;
        /// <summary> 音楽同期マネージャー。 </summary>
        private MusicSyncManager _musicSyncManager;
        /// <summary> 追跡対象のTransform。 </summary>
        private Transform _target;
        /// <summary> ロックオン対象のTransform。 </summary>
        private Transform _lockTarget;
        /// <summary> NavMeshAgentコンポーネント。 </summary>
        private NavMeshAgent _agent;
        /// <summary> ヘルスエンティティ。 </summary>
        private HealthEntity _healthEntity;
        /// <summary> エネミーの移動処理。 </summary>
        private EnemyMover _enemyMover;
        /// <summary> エネミーの攻撃処理。 </summary>
        private EnemyAttack _enemyAttack;
        /// <summary> 攻撃インジケーター。 </summary>
        private AttackIndicater _attackIndicater;
        /// <summary> ロックオン状態かどうかを示すフラグ。 </summary>
        private bool _isLockOn = false;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     コンポーネントの初期化を行います。
        /// </summary>
        private void Awake()
        {
            _lockTarget = transform;
            _agent = GetComponent<NavMeshAgent>();
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
        }

        /// <summary>
        ///     オブジェクトが無効になったときに呼び出されます。
        ///     攻撃処理を破棄します。
        /// </summary>
        private void OnDisable()
        {
            _enemyAttack?.Dispose();
            _enemyAttack = null;
        }

        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     エネミーの移動処理を更新します。
        /// </summary>
        private void FixedUpdate()
        {
            if (_enemyMover == null)
            {
                Debug.Log("EnemyMoveがnullです。");
                return;
            }
            _enemyMover.MoveTo();
        }
        #endregion
    }
}

