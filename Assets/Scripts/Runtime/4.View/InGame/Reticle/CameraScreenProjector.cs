using KillChord.Runtime.Adaptor.InGame.Reticle;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Reticle
{
    using Camera = UnityEngine.Camera;

    /// <summary>
    ///     Camera を用いてワールド座標をスクリーン座標へ投影する <see cref="IScreenProjector"/> 実装。
    ///     カメラ背後および画面外の対象は投影対象外とする。
    /// </summary>
    public sealed class CameraScreenProjector : IScreenProjector
    {
        /// <summary>
        ///     投影に使用するカメラを受け取って初期化する。
        /// </summary>
        /// <param name="camera"> 投影に使用するカメラ。 </param>
        public CameraScreenProjector(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>
        ///     ワールド座標をスクリーン座標へ投影する。
        ///     カメラが無効・カメラ背後・画面外の場合は false を返す。
        /// </summary>
        /// <param name="worldPosition"> 投影元のワールド座標。 </param>
        /// <param name="screenPosition"> 投影されたスクリーン座標。 </param>
        /// <returns> 画面内へ投影できた場合は true。 </returns>
        public bool TryWorldToScreen(in Vector3 worldPosition, out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

            if (_camera == null)
            {
                return false;
            }

            Vector3 projected = _camera.WorldToScreenPoint(worldPosition);

            // z が 0 以下はカメラ背後。画面へ投影しない。
            if (projected.z <= 0f)
            {
                return false;
            }

            // 画面外はレティクルを表示しない。画面端インジケーターが必要になれば
            // このProjectorを差し替えることで対応する。
            if (projected.x < 0f || projected.x > Screen.width
                || projected.y < 0f || projected.y > Screen.height)
            {
                return false;
            }

            screenPosition = new Vector2(projected.x, projected.y);
            return true;
        }

        private readonly Camera _camera;
    }
}
