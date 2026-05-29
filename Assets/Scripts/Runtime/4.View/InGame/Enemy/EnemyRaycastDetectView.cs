using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    /// Enemy attack raycast and its warning line presentation.
    /// </summary>
    public partial class EnemyRaycastDetectView : MonoBehaviour, IEnemyRaycastDetectViewModel
    {

        /// <summary>
        /// Initializes raycast target and warning line settings.
        /// </summary>
        public void Initialize(Transform targetTransform, float attackRange)
        {
            _hitResults = new RaycastHit[_resultArraySize];
            _targetTransform = targetTransform;
            _attackRange = attackRange;

            if (!TryGetComponent(out _lineRenderer))
            {
                Debug.LogError("[EnemyRaycastDetectView] Failed to find LineRenderer.");
                return;
            }

            if (targetTransform == null)
            {
                Debug.LogError("[EnemyRaycastDetectView] Target transform is null.");
                return;
            }

            if (!targetTransform.TryGetComponent(out _targetCollider))
            {
                Debug.LogError("[EnemyRaycastDetectView] Target collider is missing.");
                return;
            }

            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
            HideWarningInternal();

#if UNITY_EDITOR
            _initializedFlg = true;
#endif
        }

        /// <summary>
        /// Returns whether the enemy's current attack ray can reach the target.
        /// </summary>
        public bool CanRaycastHitTarget => CheckCurrentAttackRaycastHitTarget();

        /// <summary>
        /// Returns whether a free ray from the given position can reach the target.
        /// This is intended for search logic and never uses the locked warning direction.
        /// </summary>
        public bool CheckCanRaycastHitTarget(Vector3 sourcePosition)
        {
            return CheckRaycastHitTarget(sourcePosition);
        }

        /// <summary>
        /// Starts tracking the target with the warning line.
        /// </summary>
        public void StartTrackingWarning()
        {
            if (!IsReadyForLineUpdate()) return;

            _warningDisplayState = WarningDisplayState.Tracking;
            _currentLineColor = Color.yellow;
            UpdateWarningLine();
        }

        /// <summary>
        /// Locks the current warning direction while keeping the line length fixed.
        /// </summary>
        public void LockWarningDirection()
        {
            if (!IsReadyForLineUpdate()) return;

            FreezeCurrentRayDirection();
            _warningDisplayState = WarningDisplayState.Locked;
            _currentLineColor = Color.red;
            UpdateWarningLine();
        }

        /// <summary>
        /// Hides the warning line and clears the locked direction.
        /// </summary>
        public void HideWarning()
        {
            HideWarningInternal();
        }

        [SerializeField, Tooltip("Maximum number of raycast hits stored per query.")]
        private int _resultArraySize = 8;
        [SerializeField, Tooltip("Layers that block or receive the enemy attack ray.")]
        private LayerMask _hitLayers;
        [SerializeField] private LineRenderer _lineRenderer;

        private RaycastHit[] _hitResults;
        private Collider _targetCollider;
        private Transform _targetTransform;
        private float _attackRange;
        private WarningDisplayState _warningDisplayState;
        private Vector3 _lockedRayDirection;
        private Color _currentLineColor;

#if UNITY_EDITOR
        private bool _initializedFlg;
#endif

        private bool CheckCurrentAttackRaycastHitTarget()
        {
            return CheckRaycastHitTarget(transform.position);
        }

        private bool CheckRaycastHitTarget(Vector3 sourcePosition)
        {
            if (!IsReadyForRaycast())
            {
                Debug.LogError("[EnemyRaycastDetectView] Raycast is not initialized.");
                return false;
            }

            int hitCount = CastAndGetHitCount(sourcePosition);
            if (hitCount <= 0)
            {
                return false;
            }

            RaycastHit hit = FindClosestHit(hitCount);
            return hit.colliderEntityId == _targetCollider.GetEntityId();
        }

        private int CastAndGetHitCount(Vector3 sourcePosition)
        {
            Ray ray = CreateRay(sourcePosition);
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

        private void LateUpdate()
        {
            if (_warningDisplayState != WarningDisplayState.Tracking) return;
            if (!IsReadyForLineUpdate()) return;

            UpdateWarningLine();
        }

        private void UpdateWarningLine()
        {
            Ray ray = CreateRay(transform.position);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;
            _lineRenderer.material.SetColor("_EmissionColor", _currentLineColor);
            _lineRenderer.SetPosition(0, ray.origin);
            _lineRenderer.SetPosition(1, ray.origin + ray.direction * _attackRange);
        }

        private Ray CreateRay(Vector3 sourcePosition)
        {
            if (ShouldUseLockedDirection(sourcePosition))
            {
                return new Ray(sourcePosition, _lockedRayDirection);
            }

            Vector3 targetPoint = GetRayTargetPoint(sourcePosition);
            Vector3 direction = targetPoint - sourcePosition;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return new Ray(sourcePosition, Vector3.zero);
            }

            return new Ray(sourcePosition, direction.normalized);
        }

        private bool ShouldUseLockedDirection(Vector3 sourcePosition)
        {
            return _warningDisplayState == WarningDisplayState.Locked
                && IsEnemyOrigin(sourcePosition);
        }

        private void FreezeCurrentRayDirection()
        {
            Vector3 targetPoint = GetRayTargetPoint(transform.position);
            Vector3 direction = targetPoint - transform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                _lockedRayDirection = Vector3.zero;
                _warningDisplayState = WarningDisplayState.Hidden;
                return;
            }

            _lockedRayDirection = direction.normalized;
        }

        private void HideWarningInternal()
        {
            _warningDisplayState = WarningDisplayState.Hidden;
            _lockedRayDirection = Vector3.zero;

            if (_lineRenderer == null) return;
            _lineRenderer.enabled = false;
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
            HideWarningInternal();
        }
    }
}
