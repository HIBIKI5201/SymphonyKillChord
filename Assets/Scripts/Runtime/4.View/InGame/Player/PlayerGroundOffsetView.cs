using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーの見た目を地面の高さに合わせて上下させる。
    /// </summary>
    public sealed class PlayerGroundOffsetView : MonoBehaviour
    {
        /// <summary>
        ///     見た目の初期位置を記録する。
        /// </summary>
        private void Awake()
        {
            _initRendererLocalPosition = _rendererTransform.localPosition;
        }

        /// <summary>
        ///     下方向へ Ray を飛ばし、地面の高さにオフセットを足した位置へ見た目を合わせる。
        /// </summary>
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

        /// <summary>
        ///     地面を探す Ray をシーンビューに表示する。
        /// </summary>
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
