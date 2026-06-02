using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    /// 敵の攻撃用レイキャストと警告ライン表示を担当するViewです。
    /// </summary>
    public partial class EnemyRaycastDetectView : MonoBehaviour, IEnemyRaycastDetectViewModel
    {

        /// <summary>
        /// レイキャスト対象と警告ラインの初期設定を行います。
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
        /// 現在の敵位置からの攻撃レイがターゲットに届くかを返します。
        /// </summary>
        public bool CanRaycastHitTarget => CheckCurrentAttackRaycastHitTarget();

        /// <summary>
        /// 指定位置からの自由なレイがターゲットに届くかを返します。
        /// 探索用途を想定しており、固定済みの警告方向は使用しません。
        /// </summary>
        public bool CheckCanRaycastHitTarget(Vector3 sourcePosition)
        {
            return CheckRaycastHitTarget(sourcePosition);
        }

        /// <summary>
        /// 警告ラインのターゲット追従を開始します。
        /// </summary>
        public void StartTrackingWarning()
        {
            if (!IsReadyForLineUpdate()) return;

            _warningDisplayState = WarningDisplayState.Tracking;
            _currentLineColor = Color.yellow;
            UpdateWarningLine();
        }

        /// <summary>
        /// 現在の警告方向を固定し、ラインの長さを維持します。
        /// </summary>
        public void LockWarningDirection()
        {
            if (!IsReadyForLineUpdate()) return;

            if (!FreezeCurrentRayDirection())
            {
                _warningDisplayState = WarningDisplayState.Hidden;
                return;
            }

            _warningDisplayState = WarningDisplayState.Locked;
            _currentLineColor = Color.red;
            UpdateWarningLine();
        }

        /// <summary>
        /// 警告ラインを非表示にし、固定方向を解除します。
        /// </summary>
        public void HideWarning()
        {
            HideWarningInternal();
        }

        [SerializeField, Tooltip("Maximum number of raycast hits stored per query.")]
        private int _resultArraySize = 8;
        [SerializeField, Tooltip("Layers that block or receive the enemy attack ray.")]
        private LayerMask _hitLayers;
         private LineRenderer _lineRenderer;

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

        /// <summary>
        /// 現在の敵位置からの攻撃レイがターゲットに命中するかを判定します。
        /// </summary>
        private bool CheckCurrentAttackRaycastHitTarget()
        {
            return CheckRaycastHitTarget(transform.position);
        }

        /// <summary>
        /// 指定位置から飛ばしたレイが最初にターゲットへ到達するかを判定します。
        /// </summary>
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

        /// <summary>
        /// 指定位置からレイを飛ばし、記録されたヒット数を返します。
        /// </summary>
        private int CastAndGetHitCount(Vector3 sourcePosition)
        {
            Ray ray = CreateRay(sourcePosition);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return 0;
            }

            return Physics.RaycastNonAlloc(ray, _hitResults, _attackRange, _hitLayers);
        }

        /// <summary>
        /// 現在のレイキャスト結果から最も近いヒットを取得します。
        /// </summary>
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

        /// <summary>
        /// ターゲット追従中は毎フレーム警告ラインを更新します。
        /// </summary>
        private void LateUpdate()
        {
            if (_warningDisplayState != WarningDisplayState.Tracking) return;
            if (!IsReadyForLineUpdate()) return;

            UpdateWarningLine();
        }

        /// <summary>
        /// 現在のレイ情報をもとに警告ラインの位置と色を更新します。
        /// </summary>
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

        /// <summary>
        /// 現在のターゲット位置、または固定方向に向かうレイを生成します。
        /// </summary>
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

        /// <summary>
        /// 警告ラインが固定済み方向を再利用すべきかを返します。
        /// </summary>
        private bool ShouldUseLockedDirection(Vector3 sourcePosition)
        {
            return _warningDisplayState == WarningDisplayState.Locked
                && IsEnemyOrigin(sourcePosition);
        }

        /// <summary>
        /// 警告ラインを固定表示するために現在のレイ方向を保存します。
        /// </summary>
        private bool FreezeCurrentRayDirection()
        {
            Vector3 targetPoint = GetRayTargetPoint(transform.position);
            Vector3 direction = targetPoint - transform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                _lockedRayDirection = Vector3.zero;
                _warningDisplayState = WarningDisplayState.Hidden;
                return false;
            }

            _lockedRayDirection = direction.normalized;
            return true;
        }

        /// <summary>
        /// 警告ラインを非表示にし、保持している状態をリセットします。
        /// </summary>
        private void HideWarningInternal()
        {
            _warningDisplayState = WarningDisplayState.Hidden;
            _lockedRayDirection = Vector3.zero;

            if (_lineRenderer == null) return;
            _lineRenderer.enabled = false;
        }

        /// <summary>
        /// 指定した始点がこの敵自身の位置かを返します。
        /// </summary>
        private bool IsEnemyOrigin(Vector3 sourcePosition)
        {
            return (sourcePosition - transform.position).sqrMagnitude <= 0.0001f;
        }

        /// <summary>
        /// レイキャストに必要な参照が初期化済みかを返します。
        /// </summary>
        private bool IsReadyForRaycast()
        {
            return _targetTransform != null && _targetCollider != null && _hitResults != null;
        }

        /// <summary>
        /// 警告ラインを更新できる状態かを返します。
        /// </summary>
        private bool IsReadyForLineUpdate()
        {
            return IsReadyForRaycast() && _lineRenderer != null ;
        }

        /// <summary>
        /// レイの到達先として使うターゲットコライダー上の最近接点を取得します。
        /// </summary>
        private Vector3 GetRayTargetPoint(Vector3 sourcePosition)
        {
            return _targetCollider.ClosestPoint(sourcePosition);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 現在の敵レイがターゲットへ届くかをデバッグ用 Gizmo で描画します。
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!_initializedFlg || _targetCollider == null) return;

            Gizmos.color = CheckCurrentAttackRaycastHitTarget() ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, GetRayTargetPoint(transform.position));
        }

        /// <summary>
        /// 指定位置からターゲット位置までのデバッグ用 Gizmo 線を描画します。
        /// </summary>
        public void DrawGizmoLineToTarget(Vector3 source)
        {
            if (_targetCollider == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(source, GetRayTargetPoint(source));
        }
#endif

        /// <summary>
        /// このコンポーネントが無効化された際に警告ラインを非表示にします。
        /// </summary>
        private void OnDisable()
        {
            HideWarningInternal();
        }
    }
}
