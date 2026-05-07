using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵から射線を通し、指定対象に直撃できるか判定するビュー。
    /// </summary>
    public class EnemyRaycastDetectView : MonoBehaviour, IEnemyRaycastDetectViewModel
    {
        public void Initialize(Transform targetTransform, float attackRange)
        {
            _hitResults = new RaycastHit[_resultArraySize];
            _targetTransform = targetTransform; 
            _attackRange = attackRange;

            if(targetTransform == null)
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象transformがNULL。");
                return;
            }
            if (!targetTransform.TryGetComponent<Collider>(out _targetCollider))
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象がColliderを持っていない。");
                return;
            }
#if UNITY_EDITOR
            _initializedFlg = true;
#endif
        }
        public bool CanRaycastHitTarget => CheckCanRaycastHitTarget(transform.position);

        /// <summary>
        ///     始点からの射線が対象に直撃できるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool CheckCanRaycastHitTarget(Vector3 sourcePosition)
        {
            if(_targetTransform == null || _targetCollider == null)
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象の取得が出来ていない。");
                return false;
            }
            int hitCount = CastAndGetHitCount(sourcePosition);
            if (hitCount > 0)
            {
                RaycastHit hit = FindClosestHit(hitCount);
                // 最初に当たった対象が指定された対象の場合、trueを返却
                return hit.colliderEntityId == _targetCollider.GetEntityId();
            }
            return false;
        }

        [SerializeField, Tooltip("射線判定結果の最大保持数")]
        private int _resultArraySize = 8;
        [SerializeField]
        private EnemyMoveView _enemyMoveView;
        [SerializeField, Tooltip("敵の攻撃が当たるレイヤー")]
        private LayerMask _hitLayers;

        private RaycastHit[] _hitResults; 
        private Collider _targetCollider;
        private Transform _targetTransform;
        private float _attackRange;
#if UNITY_EDITOR
        private bool _initializedFlg = false;
#endif


        /// <summary>
        ///     射線判定を行い、当たった対象の数を返却。
        /// </summary>
        /// <returns></returns>
        private int CastAndGetHitCount(Vector3 sourcePosition)
        {
            Ray ray = new Ray(sourcePosition, (_targetTransform.position - sourcePosition).normalized);
            return Physics.RaycastNonAlloc(ray, _hitResults, _attackRange, _hitLayers);
        }

        /// <summary>
        ///     Raycastが当たった最も近い対象を取得する
        /// </summary>
        /// <returns></returns>
        private RaycastHit FindClosestHit(int hitCount)
        {
            if (hitCount == 0)
                return default;

            int closestIndex = 0;
            float minDistance = _hitResults[0].distance;

            for (int i = 1; i < hitCount; i++)
            {
                float dist = _hitResults[i].distance;

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }

            return _hitResults[closestIndex];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_initializedFlg) return;
            Gizmos.color = CheckCanRaycastHitTarget(transform.position) ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, _targetTransform.position);
        }
        public void DrawGizmoLineToTarget(Vector3 source)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(source, _targetTransform.position);
        }
#endif
    }
}
