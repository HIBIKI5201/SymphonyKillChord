using KillChord.Runtime.Application;
using KillChord.Runtime.View.InGame;
using KillChord.Runtime.View.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     敵から射線を通し、指定対象に直撃できるか判定するビュー。
    /// </summary>
    public class EnemyRaycastDetectView : MonoBehaviour, IEnemyRaycastDetector
    {
        public void Initialize(Transform targetTransform, float attackRange)
        {
            _hitResults = new RaycastHit[_resultArraySize];
            _targetTransform = targetTransform; 
            _targetCollider = targetTransform.GetComponent<Collider>();
            _attackRange = attackRange;
#if UNITY_EDITOR
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = _lineMaterial;
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.01f;
#endif
        }

        /// <summary>
        ///     敵からの射線が対象に直撃できるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool CanRaycastHitTarget()
        {
            int hitCount = CastAndGetHitCount();
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

        /// <summary>
        ///     射線判定を行い、当たった対象の数を返却。
        /// </summary>
        /// <returns></returns>
        private int CastAndGetHitCount()
        {
            Ray ray = new Ray(transform.position, (_targetTransform.position - transform.position).normalized);
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
        [SerializeField] Material _lineMaterial;
        private LineRenderer _lineRenderer;
        private float _raycastFrequency = 0.2f;
        private float _raycastTimer = 0;
        private void Update()
        {
            if (!_lineRenderer) return;
            // 敵の照準線を描画する
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _targetTransform.position);
            if (_raycastTimer > _raycastFrequency)
            {
                bool rayHitsPlayer = CanRaycastHitTarget();
                _lineRenderer.startColor = rayHitsPlayer ? Color.red : Color.green;
                _lineRenderer.endColor = rayHitsPlayer ? Color.red : Color.green;
                _raycastTimer = 0;
            }
            _raycastTimer += Time.deltaTime;
        }
#endif

    }
}
