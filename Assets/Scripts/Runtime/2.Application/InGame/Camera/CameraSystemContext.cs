using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの1フレーム分の更新に必要なコンテキストデータを保持するref構造体。
    /// </summary>
    public readonly ref struct CameraSystemContext
    {
        /// <summary>
        ///     カメラシステムの更新に必要なコンテキストデータを受け取り初期化するコンストラクタ。
        /// </summary>
        /// <param name="followPosition"> 追従対象のワールド座標。</param>
        /// <param name="input"> 視点操作の入力値。</param>
        /// <param name="moveInput"> 移動操作の入力値。</param>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        public CameraSystemContext(
            in Vector3 followPosition,
            Vector2 input,
            Vector2 moveInput,
            float deltaTime)
        {
            FollowPosition = followPosition;
            Input = input;
            MoveInput = moveInput;
            DeltaTime = deltaTime;
        }

        /// <summary> 追従対象のワールド座標。 </summary>
        public readonly Vector3 FollowPosition;

        /// <summary> 視点操作の入力値。 </summary>
        public readonly Vector2 Input;

        /// <summary> 移動操作の入力値。 </summary>
        public readonly Vector2 MoveInput;

        /// <summary> 前フレームからの経過時間。 </summary>
        public readonly float DeltaTime;
    }
}
