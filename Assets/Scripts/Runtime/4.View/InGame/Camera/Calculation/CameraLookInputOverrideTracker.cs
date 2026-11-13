using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     ロックオン中の視点入力を見て、ロックオンによるカメラ回転を止めるかどうかを判定するクラス。
    ///     視点入力が止まった直後に回転が戻らないよう、再開までの待ち時間も管理する。
    /// </summary>
    public sealed class CameraLookInputOverrideTracker
    {
        /// <summary>
        ///     カメラ設定を受け取り、判定を初期化する。
        /// </summary>
        /// <param name="config"> カメラ設定。 </param>
        public CameraLookInputOverrideTracker(CameraConfig config)
        {
            _config = config;
        }

        /// <summary>
        ///     ロックオンの開始・解除時などに、再開までの待ち時間を消す。
        /// </summary>
        public void Reset()
        {
            _resumeTimer = 0f;
        }

        /// <summary>
        ///     今フレームの視点入力が、ロックオンによる回転を止めるしきい値以上かを返す。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <returns> しきい値以上の視点入力がある場合は true。 </returns>
        public bool IsLookInputActive(in CameraUpdateContext context)
        {
            float threshold = _config.LockOnLookInputCancelThreshold;
            return context.Input.sqrMagnitude >= threshold * threshold;
        }

        /// <summary>
        ///     1フレーム分の時間を進め、ロックオンによるカメラ回転を止めるべきかを返す。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <returns> 視点入力中、または入力が止まってから再開を待っている間は true。 </returns>
        public bool Update(in CameraUpdateContext context)
        {
            if (IsLookInputActive(context))
            {
                _resumeTimer = _config.LockOnLookInputResumeDelay;
                return true;
            }

            if (_resumeTimer <= 0f)
            {
                return false;
            }

            _resumeTimer = Mathf.Max(0f, _resumeTimer - context.DeltaTime);
            return true;
        }

        private readonly CameraConfig _config;
        private float _resumeTimer;
    }
}
