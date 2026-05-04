using KillChord.Runtime.Application.InGame.Camera;
using KillChord.Runtime.Application;
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
        ///     コンストラクタ。
        /// </summary>
        /// <param name="application">カメラシステムアプリケーション。</param>
        public CameraSystemPresenter(CameraSystemApplication application)
        {
            _application = application;
        }

        /// <summary>
        ///     カメラシステムを更新し、結果の回転と位置を返す。
        /// </summary>
        /// <param name="followPosition">追従対象のワールド座標。</param>
        /// <param name="rawInput">生の入力値。</param>
        /// <param name="deltaTime">前フレームからの経過時間。</param>
        /// <param name="resultRotation">計算結果のカメラ回転。</param>
        /// <param name="resultPosition">計算結果のカメラ位置。</param>
        public void Update(
            in Vector3 followPosition,
            in Vector2 rawInput,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
#if UNITY_STANDALONE_WIN
            Vector2 input = ApplyInvert(rawInput);
#else
            Vector2 Input = rawInput;
#endif

            CameraSystemContext context = new(
                 followPosition,
                 input,
                 deltaTime
            );
            _application.Update(context, out resultRotation, out resultPosition);
        }

#if UNITY_STANDALONE_WIN
        /// <summary>
        ///     設定に基づき入力の垂直・水平反転を適用する。
        /// </summary>
        /// <param name="input">反転前の入力値。</param>
        /// <returns>反転処理後の入力値。</returns>
        private Vector2 ApplyInvert(Vector2 input)
        {
            if (_application.IsInvertVertical)
            {
                input.x = -input.x;
            }
            if (_application.IsInvertHorizontal)
            {
                input.y = -input.y;
            }
            return input;
        }
#endif

        private readonly CameraSystemApplication _application;
    }
}
