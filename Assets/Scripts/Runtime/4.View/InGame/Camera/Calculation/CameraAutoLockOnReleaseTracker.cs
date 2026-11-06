using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     オートロックオンの解除条件（非命中時間・視野外・強い視点操作）を判定するクラス。
    ///     MonoBehaviourに依存せず、フレームごとの入力を渡すだけで解除判定を検証できる。
    /// </summary>
    public sealed class CameraAutoLockOnReleaseTracker
    {
        /// <summary>
        ///     カメラ設定と視点操作の解除判定クラスを受け取り、判定を初期化する。
        /// </summary>
        /// <param name="config"> カメラ設定。 </param>
        /// <param name="breakTracker"> 強い視点操作によるロックオン解除判定クラス。 </param>
        public CameraAutoLockOnReleaseTracker(CameraConfig config, CameraLockOnBreakTracker breakTracker)
        {
            _config = config;
            _breakTracker = breakTracker;
        }

        /// <summary> 最後に対象へ命中してからの経過時間。 </summary>
        public float IdleTime => _idleTimer;

        /// <summary> 視野外による解除を猶予する残り時間。 </summary>
        public float ViewportGraceRemaining => _viewportGraceTimer;

        /// <summary> 蓄積中の視点操作の向き。画面の右と上を正とする。 </summary>
        public Vector2 LookDirection => _breakTracker.BreakDirection;

        /// <summary>
        ///     ロックオンの開始・解除時に、蓄積した判定状態を初期化する。
        /// </summary>
        public void Reset()
        {
            _breakTracker.Reset();
            _idleTimer = 0f;
            _viewportGraceTimer = 0f;
        }

        /// <summary>
        ///     攻撃が対象へ命中した時に、判定状態を初期化したうえで視野外解除の猶予を与える。
        /// </summary>
        public void ResetOnHit()
        {
            Reset();
            _viewportGraceTimer = _config.AutoLockOnViewportGraceDuration;
        }

        /// <summary>
        ///     対象を切り替えた直後も追従できるよう、視野外解除の猶予だけを与え直す。
        ///     非命中時間と視点操作の蓄積は変えない。
        /// </summary>
        public void ExtendViewportGrace()
        {
            _viewportGraceTimer = _config.AutoLockOnViewportGraceDuration;
        }

        /// <summary>
        ///     強い視点操作で対象を切り替えた後に、視点操作の蓄積を初期化して視野外解除の猶予を与え直す。
        ///     非命中時間は変えない。
        /// </summary>
        public void ResetOnLookSwitch()
        {
            _breakTracker.Reset();
            ExtendViewportGrace();
        }

        /// <summary>
        ///     1フレーム分の時間を進め、オートロックオンを解除するべきかとその理由を返す。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <param name="isTargetWithinViewport"> ロックオン対象が有効ビューポート内にあるかどうか。 </param>
        /// <returns> 解除する理由。解除しない場合は <see cref="CameraAutoLockOnReleaseReason.None"/>。 </returns>
        public CameraAutoLockOnReleaseReason Update(in CameraUpdateContext context, bool isTargetWithinViewport)
        {
            _idleTimer += context.DeltaTime;
            _viewportGraceTimer = Mathf.Max(0f, _viewportGraceTimer - context.DeltaTime);

            if (_idleTimer >= _config.AutoLockOnReleaseDelay)
            {
                return CameraAutoLockOnReleaseReason.Idle;
            }

            if (_viewportGraceTimer <= 0f && !isTargetWithinViewport)
            {
                return CameraAutoLockOnReleaseReason.OutOfViewport;
            }

            // 視点操作の蓄積は、ほかの条件で解除しないフレームだけ進める。
            return _breakTracker.Update(context)
                ? CameraAutoLockOnReleaseReason.StrongLook
                : CameraAutoLockOnReleaseReason.None;
        }

        private readonly CameraConfig _config;
        private readonly CameraLockOnBreakTracker _breakTracker;
        private float _idleTimer;
        private float _viewportGraceTimer;
    }
}
