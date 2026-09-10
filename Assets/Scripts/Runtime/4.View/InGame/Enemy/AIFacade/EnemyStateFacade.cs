using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵AI用ファサード：状態判定系。
    /// </summary>
    public class EnemyStateFacade : MonoBehaviour, IEnemyStateFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aiController"></param>
        /// <param name="target"></param>
        /// <param name="raycastDetectView"></param>
        /// <param name="battleState"></param>
        public void Initialize(EnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView, EnemyBattleState battleState)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
            _battleState = battleState;
        }
        /// <summary> 目標が自分の攻撃範囲内か </summary>
        public bool IsTargetInAttackRange => _aiController.IsPlayerInAttackRange(transform.position, _target.position);

        /// <summary> 目標と自分の間に障害物がないか </summary>
        public bool IsSightClearToAim => _raycastDetectView.CanRaycastHitTarget;

        /// <summary> 攻撃中であるか </summary>
        public bool IsAttacking => _aiController.IsAttacking;
        /// <summary> 硬直中か。 </summary>
        public bool IsStunned => _battleState.IsStunned;
        /// <summary> 敵の戦闘AIが有効な状態か </summary>
        public bool IsBattleAiActivated => _battleState.IsBattleAIActivated;
        /// <summary> プレイヤーを発見済みか </summary>
        public bool IsDiscovered => _battleState.IsDiscovered;
        /// <summary> プレイヤーが視野角・索敵距離・視線のすべてを満たし発見可能な状態か </summary>
        public bool IsPlayerDiscoverable
        {
            get
            {
                if (_target == null) return false;

                // 水平面での方向のみで視野角を判定する(上下は無視)
                Vector3 toTarget = _target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.magnitude > _sightRange) return false;

                float angle = Vector3.Angle(transform.forward, toTarget);
                if (angle > _viewAngleDegrees) return false;

                // 索敵の視線判定は攻撃射程ではなく索敵距離を基準に行う
                return _raycastDetectView.CheckCanRaycastHitTargetAtRange(transform.position, _sightRange);
            }
        }

        [SerializeField, Tooltip("索敵に使う視野角(度)。正面から左右それぞれこの角度までを視野内とする。"), Range(0f, 180f)]
        private float _viewAngleDegrees = 60f;
        [SerializeField, Tooltip("索敵距離(m)。"), Min(0f)]
        private float _sightRange = 15f;
        [SerializeField, Tooltip("未発見時、周囲を確認するために向きを変える速さ(度/秒)。"), Min(0f)]
        private float _lookAroundRotationSpeed = 60f;
        private EnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;
        private EnemyBattleState _battleState;
        private NavMeshAgent _navMeshAgent;
        private float _lookAroundTargetYaw;
        private bool _hasLookAroundTarget;

        /// <summary>
        ///     参照コンポーネントの取得処理。
        /// </summary>
        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        ///     硬直発生。
        /// </summary>
        public void Stunned()
        {
            // 一時。今後はAnimation Controllerで制御するはず
            Debug.Log("[EnemyStateFacade] クリティカルにより、敵硬直発生。");
        }
        /// <summary>
        ///     硬直回復。
        /// </summary>
        public void StunRecover()
        {
            _battleState.StunRecover();
        }
        /// <summary>
        ///     プレイヤーを発見済みにする。
        /// </summary>
        public void Discover()
        {
            _battleState.Discover();
        }
        /// <summary>
        ///     未発見時、その場で周囲を確認するように少しずつ向きを変える。
        /// </summary>
        public void LookAround()
        {
            if (_navMeshAgent != null)
            {
                // NavMeshAgentによる向きの自動上書きを止め、見回しの回転を優先する
                _navMeshAgent.updateRotation = false;
            }

            Quaternion targetRotation = Quaternion.Euler(0f, _lookAroundTargetYaw, 0f);
            if (!_hasLookAroundTarget || Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                _lookAroundTargetYaw = transform.eulerAngles.y + Random.Range(-150f, 150f);
                _hasLookAroundTarget = true;
                targetRotation = Quaternion.Euler(0f, _lookAroundTargetYaw, 0f);
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _lookAroundRotationSpeed * Time.deltaTime);
        }
    }
}
