using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Utility.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵AI用ファサード：状態判定系。
    /// </summary>
    public class EnemyStateFacade : StateFacadeBase
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aiController"> 敵のAIコントローラー。 </param>
        /// <param name="target"> 攻撃対象。 </param>
        /// <param name="raycastDetectView"> 射線判定に使う View。 </param>
        /// <param name="battleState"> 硬直・発見状態を持つ戦闘状態。 </param>
        public void Initialize(EnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView, EnemyBattleState battleState)
        {
            InitializeCommon(aiController, target, raycastDetectView);
            _battleState = battleState;
        }

        /// <summary> 硬直中か。 </summary>
        public override bool IsStunned => _battleState.IsStunned;
        /// <summary> 敵の戦闘AIが有効な状態か </summary>
        public bool IsBattleAiActivated => _battleState.IsBattleAIActivated;
        /// <summary> プレイヤーを発見済みか </summary>
        public bool IsDiscovered => _battleState.IsDiscovered;
        /// <summary> プレイヤーが視野角・索敵距離・視線のすべてを満たし発見可能な状態か </summary>
        public bool IsPlayerDiscoverable
        {
            get
            {
                if (Target == null) return false;

                // 水平面での方向のみで視野角を判定する(上下は無視)
                Vector3 toTarget = Target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.magnitude > _sightRange) return false;

                float angle = Vector3.Angle(transform.forward, toTarget);
                if (angle > _viewAngleDegrees) return false;

                // 索敵の視線判定は攻撃射程ではなく索敵距離を基準に行う
                return RaycastDetectView.CheckCanRaycastHitTargetAtRange(transform.position, _sightRange);
            }
        }

        [SerializeField, Tooltip("索敵に使う視野角(度)。正面から左右それぞれこの角度までを視野内とする。"), Range(0f, 180f)]
        private float _viewAngleDegrees = 60f;
        [SerializeField, Tooltip("索敵距離(m)。"), Min(0f)]
        private float _sightRange = 15f;
        [SerializeField, Tooltip("未発見時、周囲を確認するために向きを変える速さ(度/秒)。"), Min(0f)]
        private float _lookAroundRotationSpeed = 60f;
        [SerializeField, Tooltip("クリティカル被弾時、硬直状態が継続する時間(秒)。"), Min(0f)]
        private float _stunDurationSeconds = 2f;
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
        public override void Stunned()
        {
            // 一時。今後はAnimation Controllerで制御するはず
            DevLog.Log("[EnemyStateFacade] クリティカルにより、敵硬直発生。");
            // 硬直時間経過後に自動回復させる。多重予約を避けるため、既存の予約は一度キャンセルする。
            CancelInvoke(nameof(StunRecover));
            Invoke(nameof(StunRecover), _stunDurationSeconds);
        }
        /// <summary>
        ///     硬直回復。
        /// </summary>
        public override void StunRecover()
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
