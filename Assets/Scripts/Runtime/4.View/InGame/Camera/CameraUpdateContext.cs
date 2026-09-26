using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     Camera View の計算に必要な1フレーム分の入力データ。
    /// </summary>
    public readonly struct CameraUpdateContext
    {
        /// <summary>
        ///     1フレーム分の入力データを初期化する。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="input"> 視点操作の入力値。</param>
        /// <param name="moveInput"> 移動操作の入力値。</param>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        public CameraUpdateContext(
            in Vector3 followPosition,
            in Vector2 input,
            in Vector2 moveInput,
            float deltaTime)
        {
            FollowPosition = followPosition;
            Input = input;
            MoveInput = moveInput;
            DeltaTime = deltaTime;
        }

        /// <summary> 追従対象のワールド座標。 </summary>
        public Vector3 FollowPosition { get; }

        /// <summary> 視点操作の入力値。 </summary>
        public Vector2 Input { get; }

        /// <summary> 移動操作の入力値。 </summary>
        public Vector2 MoveInput { get; }

        /// <summary> 前フレームからの経過時間。 </summary>
        public float DeltaTime { get; }
    }
}
