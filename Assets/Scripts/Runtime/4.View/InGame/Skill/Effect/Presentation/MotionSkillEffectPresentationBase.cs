using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     LitMotionでスキルエフェクトを再生するストラテジーの基底クラス。
    ///     Tween内容は派生クラスが決定し、完了までTweenを待機する。
    /// </summary>
    public abstract class MotionSkillEffectPresentationBase : SkillEffectPresentationBase
    {
        /// <summary>
        ///     Tweenを生成し、完了まで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            CancelMotion();
            _isMotionCompleted = false;
            _motionHandle = CreateMotion(context);
            await _motionHandle.ToAwaitable(cancellationToken);

            // 最後まで再生できた場合のみ完了扱いとし、停止時に終了値を保持する。
            _isMotionCompleted = true;
        }

        /// <summary>
        ///     再生中のTweenを中断する。中断された場合のみ見た目を初期状態へ戻す。
        /// </summary>
        protected override void OnStop()
        {
            CancelMotion();

            // 完了後に初期状態へ戻すと、Tweenの結果が即座に打ち消されてしまう。
            if (_isMotionCompleted)
            {
                return;
            }

            OnRestoreState();
        }

        /// <summary>
        ///     再生するTweenを生成する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 生成したTweenのハンドルです。 </returns>
        protected abstract MotionHandle CreateMotion(in SkillEffectContext context);

        /// <summary>
        ///     停止時に見た目を初期状態へ戻す。
        /// </summary>
        protected virtual void OnRestoreState()
        {
        }

        /// <summary>
        ///     破棄時にTweenを確実に中断する。
        /// </summary>
        private void OnDestroy()
        {
            CancelMotion();
        }

        /// <summary>
        ///     再生中のTweenが存在すれば中断する。
        /// </summary>
        private void CancelMotion()
        {
            if (!_motionHandle.IsActive())
            {
                return;
            }

            _motionHandle.Cancel();
            _motionHandle = default;
        }

        private MotionHandle _motionHandle;
        private bool _isMotionCompleted;
    }
}
