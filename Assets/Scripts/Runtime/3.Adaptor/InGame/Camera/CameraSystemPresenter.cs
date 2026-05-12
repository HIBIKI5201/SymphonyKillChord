using KillChord.Runtime.Application.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Camera
{
    /// <summary>
    ///     カメラシステムのプレゼンター。
    ///     アプリケーション層へカメラ更新を委譲する。
    ///     Windows の場合は入力の反転操作を適用する。
    /// </summary>
    public sealed class CameraSystemPresenter
    {
        /// <summary>
        ///     カメラシステムアプリケーションを受け取り、プレゼンターを初期化するコンストラクタ。
        /// </summary>
        /// <param name="application"> カメラシステムアプリケーション。</param>
        public CameraSystemPresenter(CameraSystemApplication application)
        {
            _application = application;
        }

        /// <summary>
        ///     カメラシステムを更新し、結果の回転と位置を返す。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="rawInput"> 生の入力値。</param>
        /// <param name="moveInput"> 移動操作の入力値。</param>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        /// <param name="resultRotation"> 計算結果のカメラ回転。</param>
        /// <param name="resultPosition"> 計算結果のカメラ位置。</param>
        public void Update(
            in Vector3 followPosition,
            in Vector2 rawInput,
            in Vector2 moveInput,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition)
        {
            CameraSystemContext context = new(
                followPosition,
                rawInput,
                moveInput,
                deltaTime
            );
            _application.Update(context, out resultRotation, out resultPosition);
        }

        private readonly CameraSystemApplication _application;
    }
}
