using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスの3方向攻撃用レイキャストと警告ライン表示を担当するViewです。
    /// </summary>
    public partial class TripleShotRaycastDetectView : MonoBehaviour, IEnemyRaycastDetectViewModel, IRaycastDetectView
    {

        /// <summary>
        ///     レイキャスト対象と警告ラインの初期設定を行います。
        /// </summary>
        public void Initialize(Transform targetTransform, float attackRange)
        {
            // 判定結果のバッファと対象を用意する。
            _hitResults = new RaycastHit[_resultArraySize];
            _targetTransform = targetTransform;
            _attackRange = attackRange;

            // 3本分の照準線と、対象のコライダーがあるかを確認する。
            if (_lineRenderers == null || _lineRenderers.Length != AIM_LINE_COUNT)
            {
                Debug.LogError($"[TripleShotRaycastDetectView] LineRendererの数が不正:{_lineRenderers?.Length.ToString() ?? "null"}.");
                return;
            }

            if (targetTransform == null)
            {
                Debug.LogError("[TripleShotRaycastDetectView] Target transform is null.");
                return;
            }

            if (!targetTransform.TryGetComponent(out _targetCollider))
            {
                Debug.LogError("[TripleShotRaycastDetectView] Target collider is missing.");
                return;
            }

            // 照準線を2点のワールド座標で描く設定にし、非表示で始める。
            foreach(LineRenderer lineRenderer in _lineRenderers)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
            }
            CreateLineMaterials();
            HideWarningInternal();

#if UNITY_EDITOR
            _initializedFlg = true;
#endif
        }

        /// <summary>
        ///     現在の敵位置からの攻撃レイがターゲットに届くかを返します。
        /// </summary>
        public bool CanRaycastHitTarget => CheckCurrentAttackRaycastHitTarget();

        /// <summary>
        ///     指定位置からの自由なレイがターゲットに届くかを返します。
        ///     探索用途を想定しており、固定済みの警告方向は使用しません。
        /// </summary>
        public bool CheckCanRaycastHitTarget(Vector3 sourcePosition)
        {
            return CheckRaycastHitTarget(sourcePosition);
        }

        /// <summary>
        ///     警告ラインのターゲット追従を開始します。
        /// </summary>
        public void StartTrackingWarning()
        {
            if (!IsReadyForLineUpdate()) return;

            _warningDisplayState = WarningDisplayState.Tracking;
            _currentLineColor = Color.yellow;
            UpdateWarningLine();
        }

        /// <summary>
        ///     現在の警告方向を固定し、ラインの長さを維持します。
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
        ///     警告ラインを非表示にし、固定方向を解除します。
        /// </summary>
        public void HideWarning()
        {
            HideWarningInternal();
        }

        // 攻撃方向数に関する定数
        private const int AIM_LINE_COUNT = 3;
        private const int AIM_LINE_INDEX_LEFT = 0;
        private const int AIM_LINE_INDEX_CENTER = 1;
        private const int AIM_LINE_INDEX_RIGHT = 2;

        private static readonly int EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");

        [SerializeField, Tooltip("Maximum number of raycast hits stored per query.")]
        private int _resultArraySize = 8;
        [SerializeField, Tooltip("Layers that block or receive the enemy attack ray.")]
        private LayerMask _hitLayers;
        [SerializeField, Tooltip("振り角の角度")]
        private float SpreadAngleDegrees = 45f;
        [SerializeField, Tooltip("各攻撃の警告ライン用LineRenderer")]
        private LineRenderer[] _lineRenderers;

        private RaycastHit[] _hitResults;
        private Collider _targetCollider;
        private Transform _targetTransform;
        private float _attackRange;
        private WarningDisplayState _warningDisplayState;
        private Vector3 _lockedRayDirection;
        private Color _currentLineColor;
        // 照準線ごとに複製したマテリアル。破棄時に Destroy する。
        private Material[] _lineMaterials;

#if UNITY_EDITOR
        private bool _initializedFlg;
#endif

        /// <summary>
        ///     現在の敵位置からの攻撃レイがターゲットに命中するかを判定します。
        /// </summary>
        private bool CheckCurrentAttackRaycastHitTarget()
        {
            return CheckRaycastHitTarget(transform.position);
        }

        /// <summary>
        ///     指定位置から飛ばしたレイが最初にターゲットへ到達するかを判定します。
        /// </summary>
        private bool CheckRaycastHitTarget(Vector3 sourcePosition)
        {
            if (!IsReadyForRaycast())
            {
                Debug.LogError("[TripleShotRaycastDetectView] Raycast is not initialized.");
                return false;
            }

            int hitCount = CastAndGetHitCount(sourcePosition, -SpreadAngleDegrees);
            if (hitCount > 0)
            {
                RaycastHit hit = FindClosestHit(hitCount);
                if(hit.colliderEntityId == _targetCollider.GetEntityId())
                {
                    return true;
                }
            }

            hitCount = CastAndGetHitCount(sourcePosition, 0);
            if (hitCount > 0)
            {
                RaycastHit hit = FindClosestHit(hitCount);
                if (hit.colliderEntityId == _targetCollider.GetEntityId())
                {
                    return true;
                }
            }

            hitCount = CastAndGetHitCount(sourcePosition, SpreadAngleDegrees);
            if (hitCount > 0)
            {
                RaycastHit hit = FindClosestHit(hitCount);
                if (hit.colliderEntityId == _targetCollider.GetEntityId())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     指定位置からレイを飛ばし、記録されたヒット数を返します。
        /// </summary>
        private int CastAndGetHitCount(Vector3 sourcePosition, float spredAngleDegrees)
        {
            Ray ray = CreateRay(sourcePosition, spredAngleDegrees);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return 0;
            }

            return Physics.RaycastNonAlloc(ray, _hitResults, _attackRange, _hitLayers);
        }

        /// <summary>
        ///     現在のレイキャスト結果から最も近いヒットを取得します。
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
        ///     ターゲット追従中は毎フレーム警告ラインを更新します。
        /// </summary>
        private void LateUpdate()
        {
            if (_warningDisplayState != WarningDisplayState.Tracking) {
                return; 
            }
            if (!IsReadyForLineUpdate())
            {
                return;
            }
            UpdateWarningLine();
        }

        /// <summary>
        ///     現在のレイ情報をもとに警告ラインの位置と色を更新します。
        /// </summary>
        private void UpdateWarningLine()
        {
            Ray ray = CreateRay(transform.position, 0);
            if (ray.direction.sqrMagnitude <= Mathf.Epsilon)
            {
                foreach (LineRenderer renderer in _lineRenderers)
                {
                    renderer.enabled = false;
                }
                return;
            }

            _lineRenderers[AIM_LINE_INDEX_LEFT].enabled = true;
            SetLineEmissionColor(AIM_LINE_INDEX_LEFT, _currentLineColor);
            _lineRenderers[AIM_LINE_INDEX_LEFT].SetPosition(0, ray.origin);
            _lineRenderers[AIM_LINE_INDEX_LEFT].SetPosition(1, ray.origin + (Quaternion.Euler(0, -SpreadAngleDegrees, 0) * ray.direction) * _attackRange);

            _lineRenderers[AIM_LINE_INDEX_CENTER].enabled = true;
            SetLineEmissionColor(AIM_LINE_INDEX_CENTER, _currentLineColor);
            _lineRenderers[AIM_LINE_INDEX_CENTER].SetPosition(0, ray.origin);
            _lineRenderers[AIM_LINE_INDEX_CENTER].SetPosition(1, ray.origin + ray.direction * _attackRange);

            _lineRenderers[AIM_LINE_INDEX_RIGHT].enabled = true;
            SetLineEmissionColor(AIM_LINE_INDEX_RIGHT, _currentLineColor);
            _lineRenderers[AIM_LINE_INDEX_RIGHT].SetPosition(0, ray.origin);
            _lineRenderers[AIM_LINE_INDEX_RIGHT].SetPosition(1, ray.origin + (Quaternion.Euler(0, SpreadAngleDegrees, 0) * ray.direction) * _attackRange);
        }

        /// <summary>
        ///     現在のターゲット位置、または固定方向に向かうレイを生成します。
        /// </summary>
        private Ray CreateRay(Vector3 sourcePosition, float spredAngleDegrees)
        {
            if (ShouldUseLockedDirection(sourcePosition))
            {
                return new Ray(sourcePosition, Quaternion.Euler(0, spredAngleDegrees, 0) * _lockedRayDirection);
            }

            Vector3 targetPoint = GetRayTargetPoint(sourcePosition);
            Vector3 direction = targetPoint - sourcePosition;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return new Ray(sourcePosition, Vector3.zero);
            }

            return new Ray(sourcePosition, Quaternion.Euler(0, spredAngleDegrees, 0) * direction.normalized);
        }

        /// <summary>
        ///     警告ラインが固定済み方向を再利用すべきかを返します。
        /// </summary>
        private bool ShouldUseLockedDirection(Vector3 sourcePosition)
        {
            return _warningDisplayState == WarningDisplayState.Locked
                && IsEnemyOrigin(sourcePosition);
        }

        /// <summary>
        ///     警告ラインを固定表示するために現在のレイ方向を保存します。
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
        ///     警告ラインを非表示にし、保持している状態をリセットします。
        /// </summary>
        private void HideWarningInternal()
        {
            _warningDisplayState = WarningDisplayState.Hidden;
            _lockedRayDirection = Vector3.zero;

            foreach(LineRenderer renderer in _lineRenderers)
            {
                renderer.enabled = false;
            }
        }

        /// <summary>
        ///     指定した始点がこの敵自身の位置かを返します。
        /// </summary>
        private bool IsEnemyOrigin(Vector3 sourcePosition)
        {
            return (sourcePosition - transform.position).sqrMagnitude <= 0.0001f;
        }

        /// <summary>
        ///     レイキャストに必要な参照が初期化済みかを返します。
        /// </summary>
        private bool IsReadyForRaycast()
        {
            return _targetTransform != null && _targetCollider != null && _hitResults != null;
        }

        /// <summary>
        ///     警告ラインを更新できる状態かを返します。
        /// </summary>
        private bool IsReadyForLineUpdate()
        {
            return IsReadyForRaycast() && _lineRenderers != null && _lineRenderers.Length == AIM_LINE_COUNT;
        }

        /// <summary>
        ///     レイの到達先として使うターゲットコライダー上の最近接点を取得します。
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
        ///     このコンポーネントが無効化された際に警告ラインを非表示にします。
        /// </summary>
        private void OnDisable()
        {
            HideWarningInternal();
        }

        /// <summary>
        ///     実行時に複製した照準線のマテリアルを破棄します。
        /// </summary>
        private void OnDestroy()
        {
            DestroyLineMaterials();
        }

        /// <summary>
        ///     照準線ごとにマテリアルを複製して割り当てます。
        ///     毎フレーム LineRenderer.material を参照すると暗黙に複製され、破棄されないまま残るため、明示的に複製して保持します。
        /// </summary>
        private void CreateLineMaterials()
        {
            DestroyLineMaterials();

            _lineMaterials = new Material[_lineRenderers.Length];
            for (int i = 0; i < _lineRenderers.Length; i++)
            {
                Material sharedMaterial = _lineRenderers[i].sharedMaterial;
                if (sharedMaterial == null)
                {
                    continue;
                }

                _lineMaterials[i] = new Material(sharedMaterial);
                _lineRenderers[i].sharedMaterial = _lineMaterials[i];
            }
        }

        /// <summary>
        ///     指定した照準線の発光色を設定します。
        /// </summary>
        /// <param name="index"> 照準線の番号。 </param>
        /// <param name="color"> 設定する発光色。 </param>
        private void SetLineEmissionColor(int index, Color color)
        {
            if (_lineMaterials == null || index < 0 || index >= _lineMaterials.Length || _lineMaterials[index] == null)
            {
                return;
            }

            _lineMaterials[index].SetColor(EMISSION_COLOR, color);
        }

        /// <summary>
        ///     複製した照準線のマテリアルを破棄します。
        /// </summary>
        private void DestroyLineMaterials()
        {
            if (_lineMaterials == null)
            {
                return;
            }

            for (int i = 0; i < _lineMaterials.Length; i++)
            {
                if (_lineMaterials[i] != null)
                {
                    Destroy(_lineMaterials[i]);
                }
            }

            _lineMaterials = null;
        }
    }
}
