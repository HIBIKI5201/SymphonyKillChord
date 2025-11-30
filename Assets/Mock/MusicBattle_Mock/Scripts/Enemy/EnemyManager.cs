using System;
using UnityEngine;
using Mock.MusicBattle.Character;
using UnityEngine.AI;
using Mock.MusicBattle.MusicSync;
using System.Runtime.InteropServices;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵の主要な処理を一括して管理するクラス。
    ///     HP管理
    ///     ターゲット設定
    ///     移動ロジック初期化
    ///     死亡イベントの通知
    ///     など、エネミー個体に関する中核処理を提供する。
    /// </summary>
   
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyManager : MonoBehaviour, ICharacter
    {
        /// <summary>
        ///     ヘルスが 0 になったときに発火するイベント。
        ///     オブジェクトプールへ戻す処理や、
        ///     コンテナから削除する処理などがここに紐づけられる。
        /// </summary>
        public event Action OnDeath
        {
            add => _healthEntity.OnDeath += value;
            remove => _healthEntity.OnDeath -= value;
        }
        public event Action OnAttack
        {
            add => _enemyMover.OnAttack += value;
            remove => _enemyMover.OnAttack -= value;
        }
        public event Action OnOutOfRange
        {
            add => _enemyMover.OnOutOfRange += value;
            remove => _enemyMover.OnOutOfRange -= value;
        }


        /// <summary>
        ///     敵自身の Transform（ロックオン時などに参照される）
        /// </summary>
        public Transform LockTarget => _lockTarget;
        public HealthEntity HealthEntity => _healthEntity;
        public bool IsLockOn => _isLockOn;

        /// <summary>
        ///     Rigidbody やロックオン用 Transform などの初期化を行い、
        ///     初期のヘルスを設定する。
        /// </summary>
        public void SetLockOn(Transform lockon)
        {
            _isLockOn = _lockTarget == lockon;
            Debug.Log($"{gameObject.name} ロックオン対象: {(_isLockOn ? "ロックオン中" : "ロックオン解除")}");
        }

        /// <summary>
        ///     このエネミーが追従・攻撃の基準とするターゲットを設定する。
        /// </summary>
        /// <param name="target">　追従対象の Transform。　</param>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary> 音楽関係を使うクラスを初期化する。 </summary>
        public void InitMusic(MusicSyncManager music)
        {
            _musicSyncManager = music;
            _enemyAttack = new EnemyAttack(this, _musicSyncManager,
                _encountSo,_battaleSo);
        }
       
        /// <summary>
        ///     エネミーの移動処理を担当する EnemyMover を初期化する。
        ///     必要なデータ（ステータス、ターゲット、Rigidbody）が揃っていない場合は初期化を中断する。
        /// </summary>
        public void InitializeMover()
        {
            if (_enemyStatus == null || _target == null)
            {
                Debug.Log("EnemyStatus/Target is null");
                return;
            }

            _enemyMover = new EnemyMover(_target, _lockTarget, _enemyStatus,_agent,this);
        }

        /// <summary>
        ///     エネミーにダメージを与え、HP を減らす。
        ///     HP が 0 になった場合は OnDeath が発火する。
        /// </summary>
        /// <param name="damage"> 与えるダメージ量。 </param>
        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        /// <summary>
        ///     Rigidbody を用いた物理更新。
        ///     EnemyMover に移動処理を委譲する。
        /// </summary>
        private void FixedUpdate()
        {
            if (_enemyMover == null)
            {
                Debug.Log("EnemyMove is null");
                return;
            }
            _enemyMover.MoveTo();
        }

        [SerializeField, Tooltip("エネミーのステータス")]
        private EnemyStatus _enemyStatus;

        [SerializeField, Tooltip("エネミーの音楽情報")]
        private EnemyMusicSO _battaleSo;

        [SerializeField, Tooltip("エネミーの音楽情報(接敵時)")]
        private EnemyMusicSO _encountSo;

        private MusicSyncManager _musicSyncManager;
        private Transform _target;
        private Transform _lockTarget;
        private NavMeshAgent _agent;
        private HealthEntity _healthEntity;
        private EnemyMover _enemyMover;
        private EnemyAttack _enemyAttack;
        private bool _isLockOn = false;

        private void Awake()
        {
            _lockTarget = transform;
            _agent = GetComponent<NavMeshAgent>();
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
        }
    }
}
