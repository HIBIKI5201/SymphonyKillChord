using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    public sealed class PlayerGroundOffsetView : MonoBehaviour
    {
        private void Awake()
        {
            _initRendererLocalPosition = _rendererTransform.localPosition;
        }
        private void LateUpdate()
        {
            if (Physics.Raycast(_rayStartTransform.position, Vector3.down, out var result, _rayLength, _layerMask))
            {
                Vector3 position = _rendererTransform.position;
                position.y = result.point.y + _offsetY;
                _rendererTransform.position = position;
            }
            else
            {
                _rendererTransform.localPosition = _initRendererLocalPosition;
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(_rayStartTransform.position, _rayStartTransform.position + Vector3.down * _rayLength);
        }

        [Tooltip("地面を探すためのRayの始点のTransform")]
        [SerializeField] private Transform _rayStartTransform;

        [Tooltip("RayCastの射程距離")]
        [SerializeField] private float _rayLength;

        [Tooltip("RayCastが使うLayerMask")]
        [SerializeField] private LayerMask _layerMask;

        [Tooltip("地面に合わせてオフセットするRendererのTransform")]
        [SerializeField] private Transform _rendererTransform;

        [Tooltip("_rendererTransformを中心としたY軸オフセット")]
        [SerializeField] private float _offsetY;

        private Vector3 _initRendererLocalPosition;
    }
}
