using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     強い視点操作によるオートロックオン解除判定を担当するクラス。
    /// </summary>
    public sealed class CameraLockOnBreakTracker
    {
        /// <summary>
        ///     カメラ設定を受け取り、解除判定の追跡を初期化する。
        /// </summary>
        /// <param name="config"> カメラ設定。 </param>
        public CameraLockOnBreakTracker(CameraConfig config)
        {
            _config = config;
        }

        /// <summary>
        ///     蓄積中の視点操作の向き。画面の右と上を正とし、解除判定と同じ時間幅で減衰する。
        /// </summary>
        public Vector2 BreakDirection => _breakDirection;

        /// <summary>
        ///     視点入力の蓄積値を更新し、解除しきい値を超えたかを返す。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <returns> オートロックオンを解除するべき場合は true。 </returns>
        public bool Update(in CameraUpdateContext context)
        {
            float breakWindow = Mathf.Max(MIN_BREAK_WINDOW, _config.LockOnBreakWindow);
            _breakAccum -= (_breakAccum / breakWindow) * context.DeltaTime;
            _breakAccum = Mathf.Max(0f, _breakAccum);
            _breakAccum += context.Input.magnitude * _config.FollowRotationSpeed * context.DeltaTime;

            // 切り替え先を選べるよう、操作の向きも同じ減衰で蓄積する。
            _breakDirection -= (_breakDirection / breakWindow) * context.DeltaTime;
            _breakDirection += context.Input * (_config.FollowRotationSpeed * context.DeltaTime);
            return _breakAccum >= _config.LockOnBreakThreshold;
        }

        /// <summary>
        ///     蓄積状態を初期化する。
        /// </summary>
        public void Reset()
        {
            _breakAccum = 0f;
            _breakDirection = Vector2.zero;
        }

        /// <summary> 0除算を防ぐ最小の判定時間幅。 </summary>
        private const float MIN_BREAK_WINDOW = 0.0001f;

        private readonly CameraConfig _config;
        private float _breakAccum;
        private Vector2 _breakDirection;
    }
}
