using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     LitMotionでスキルエフェクトを再生するストラテジーの基底クラス。
    ///     Tween内容は派生クラスが決定する。
    /// </summary>
    public abstract class MotionSkillEffectPresentationBase : SkillEffectPresentationBase
    {
        /// <summary>
        ///     Tweenを生成して再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected override void OnPlay(in SkillEffectContext context)
        {
            CancelMotion();
            _motionHandle = CreateMotion(context);
        }

        /// <summary>
        ///     再生中のTweenを中断する。
        /// </summary>
        protected override void OnStop()
        {
            CancelMotion();
            OnRestoreState();
        }

        /// <summary>
        ///     Tweenが生存しているかで再生継続を判定する。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected override bool OnCheckPlaying(float elapsedSeconds)
        {
            return _motionHandle.IsActive();
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
    }
}
