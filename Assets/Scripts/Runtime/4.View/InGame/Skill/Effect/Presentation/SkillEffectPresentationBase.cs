using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     スキルエフェクトの再生手段を表すストラテジーの基底クラス。
    ///     ParticleSystem、VFX Graph、LitMotion、Timelineなど手段ごとに派生させる。
    ///     再生は非同期で行い、完了まで待機できるようにする。
    /// </summary>
    public abstract class SkillEffectPresentationBase : MonoBehaviour
    {
        /// <summary> 再生中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     プール生成時に一度だけ呼ばれる事前準備を行う。
        /// </summary>
        public void Prewarm()
        {
            OnPrewarm();
        }

        /// <summary>
        ///     エフェクトを再生し、完了まで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        public async Awaitable PlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            _isPlaying = true;
            try
            {
                await OnPlayAsync(context, cancellationToken);
            }
            finally
            {
                // 完了・中断・例外のいずれでも、必ず停止処理を通して状態を戻す。
                _isPlaying = false;
                OnStop();
            }
        }

        /// <summary>
        ///     プール生成時の事前準備を行う。
        /// </summary>
        protected virtual void OnPrewarm()
        {
        }

        /// <summary>
        ///     派生クラスごとの再生処理を行い、完了まで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected abstract Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken);

        /// <summary>
        ///     派生クラスごとの停止処理を行う。
        /// </summary>
        protected abstract void OnStop();

        private bool _isPlaying;
    }
}
