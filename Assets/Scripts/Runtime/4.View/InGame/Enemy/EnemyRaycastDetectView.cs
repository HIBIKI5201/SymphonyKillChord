using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    /// 敵から対象へのレイ判定と、攻撃予告ライン表示を管理する View。
    /// </summary>
    public class EnemyRaycastDetectView : MonoBehaviour, IEnemyRaycastDetectViewModel
    {
        /// <summary>
        /// 初期化する。
        /// </summary>
        public void Initialize(Transform targetTransform, float attackRange)
        {
            _hitResults = new RaycastHit[_resultArraySize];
            _targetTransform = targetTransform;
            _attackRange = attackRange;

            if (!TryGetComponent(out _lineRenderer))
            {
                Debug.LogError("[EnemyRaycastDetectView] LineRendererの取得に失敗。");
                return;
            }

            if (targetTransform == null)
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象transformがNULL。");
                return;
            }

            if (!targetTransform.TryGetComponent(out _targetCollider))
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象がColliderを持っていない。");
                return;
            }

            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;

#if UNITY_EDITOR
            _initializedFlg = true;
#endif
        }

        /// <summary>
        /// 敵本体の攻撃用レイが、現在の状態で対象へ通っているか。
        /// 1拍前以降は固定したレイを使う。
        /// </summary>
        public bool CanRaycastHitTarget => CheckCurrentAttackRaycastHitTarget();

        /// <summary>
        /// 指定位置から対象へ通常のレイが通るかを判定する。
        /// 探索用途向けで、固定レイは使わない。
        /// </summary>
        public bool CheckCanRaycastHitTarget(Vector3 sourcePosition)
        {
            return CheckRaycastHitTarget(sourcePosition, useFrozenRaycast: false);
        }

        public void Handle2BeatBefore()
        {
            if (!IsReadyForLineUpdate()) return;

            _isRaycastFrozen = false;
            _isLineVisible = true;
            _currentLineColor = Color.yellow;
            UpdateLineRenderer(Color.yellow);
        }

        public void Handle1BeatBefore()
        {
            if (!IsReadyForLineUpdate()) return;

            FreezeCurrentRaycast();
            _isLineVisible = true;
            _currentLineColor = Color.red;
            UpdateLineRenderer(Color.red);
        }

        public void HandleOnAttack()
        {
            ClearFrozenRaycast();
            _isLineVisible = false;

            if (_lineRenderer == null) return;
            _lineRenderer.enabled = false;
        }

        [SerializeField, Tooltip("RaycastHitを格納する配列サイズ")]
        private int _resultArraySize = 8;
        [SerializeField, Tooltip("レイの当たり判定を行うレイヤー")]
        private LayerMask _hitLayers;
        [SerializeField, Tooltip("ライン終点をヒット地点の少し手前で止める距離")]
        private float _lineStopOffset = 0.5f;
        [SerializeField] private LineRenderer _lineRenderer;

        private RaycastHit[] _hitResults;
        private Collider _targetCollider;
        private Transform _targetTransform;
        private Vector3 _endPoint;
        private float _attackRange;
        private bool _isRaycastFrozen;
        private Vector3 _frozenRayOrigin;
        private Vector3 _frozenRayDirection;
        private bool _isLineVisible;
        private Color _currentLineColor;

#if UNITY_EDITOR
        private bool _initializedFlg;
#endif

        private bool CheckCurrentAttackRaycastHitTarget()
        {
            return CheckRaycastHitTarget(transform.position, useFrozenRaycast: true);
        }

        private bool CheckRaycastHitTarget(Vector3 sourcePosition, bool useFrozenRaycast)
        {
            if (!IsReadyForRaycast())
            {
                Debug.LogError("[EnemyRaycastDetectView] 攻撃対象の取得が出来ていない。");
                return false;
            }

            int hitCount = CastAndGetHitCount(sourcePosition, useFrozenRaycast);
            if (hitCount <= 0)
            {
                return false;
            }

            RaycastHit hit = FindClosestHit(hitCount);
            return hit.colliderEntityId == _targetCollider.GetEntityId();
        }

        private int CastAndGetHitCount(Vector3 sourcePosition, bool useFrozenRaycast)
        {
            Ray ray = CreateRay(sourcePosition, useFrozenRaycast);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return 0;
            }

            return Physics.RaycastNonAlloc(ray, _hitResults, _attackRange, _hitLayers);
        }

        private RaycastHit FindClosestHit(int hitCount)
        {
            if (hitCount == 0)
            {
                return default;
            }

            int closestIndex = 0;
            float minDistance = _hitResults[0].distance;

            for (int i = 1; i < hitCount; i++)
            {
                float distance = _hitResults[i].distance;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return _hitResults[closestIndex];
        }

        private void UpdateLineRenderer(Color emissionColor)
        {
            Ray ray = CreateRay(transform.position, useFrozenRaycast: true);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                _lineRenderer.enabled = false;
                return;
            }

            int hitCount = Physics.RaycastNonAlloc(ray, _hitResults, _attackRange, _hitLayers);
            _endPoint = hitCount > 0
                ? CalculateLineEndPoint(FindClosestHit(hitCount), ray.direction)
                : ray.origin + ray.direction * _attackRange;

            _lineRenderer.enabled = true;
            _lineRenderer.material.SetColor("_EmissionColor", emissionColor);
            _lineRenderer.SetPosition(0, ray.origin);
            _lineRenderer.SetPosition(1, _endPoint);
        }

        private void LateUpdate()
        {
            if (!_isLineVisible || _isRaycastFrozen) return;
            if (!IsReadyForLineUpdate()) return;

            UpdateLineRenderer(_currentLineColor);
        }

        private Ray CreateRay(Vector3 sourcePosition, bool useFrozenRaycast)
        {
            if (useFrozenRaycast && _isRaycastFrozen && IsEnemyOrigin(sourcePosition))
            {
                return new Ray(_frozenRayOrigin, _frozenRayDirection);
            }

            Vector3 targetPoint = GetRayTargetPoint(sourcePosition);
            Vector3 direction = targetPoint - sourcePosition;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return new Ray(sourcePosition, Vector3.zero);
            }

            return new Ray(sourcePosition, direction.normalized);
        }

        private void FreezeCurrentRaycast()
        {
            Vector3 sourcePosition = transform.position;
            Vector3 targetPoint = GetRayTargetPoint(sourcePosition);
            Vector3 direction = targetPoint - sourcePosition;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                ClearFrozenRaycast();
                return;
            }

            _frozenRayOrigin = sourcePosition;
            _frozenRayDirection = direction.normalized;
            _isRaycastFrozen = true;
        }

        private void ClearFrozenRaycast()
        {
            _isRaycastFrozen = false;
            _frozenRayOrigin = Vector3.zero;
            _frozenRayDirection = Vector3.zero;
        }

        private bool IsEnemyOrigin(Vector3 sourcePosition)
        {
            return (sourcePosition - transform.position).sqrMagnitude <= 0.0001f;
        }

        private bool IsReadyForRaycast()
        {
            return _targetTransform != null && _targetCollider != null && _hitResults != null;
        }

        private bool IsReadyForLineUpdate()
        {
            return IsReadyForRaycast() && _lineRenderer != null;
        }

        private Vector3 GetRayTargetPoint(Vector3 sourcePosition)
        {
            return _targetCollider.ClosestPoint(sourcePosition);
        }

        private Vector3 CalculateLineEndPoint(RaycastHit hit, Vector3 direction)
        {
            float offset = Mathf.Max(0f, _lineStopOffset);
            return hit.point - direction * Mathf.Min(offset, hit.distance);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_initializedFlg || _targetCollider == null) return;

            Gizmos.color = CheckCurrentAttackRaycastHitTarget() ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, GetRayTargetPoint(transform.position));
        }

        public void DrawGizmoLineToTarget(Vector3 source)
        {
            if (_targetCollider == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(source, GetRayTargetPoint(source));
        }
#endif

        private void OnDisable()
        {
            ClearFrozenRaycast();
            _isLineVisible = false;

            if (_lineRenderer == null) return;
            _lineRenderer.enabled = false;
        }
    }
}
