using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.Utility.Persistent;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     クリティカル成立時のみ追加演出を再生するストラテジー。
    ///     ダメージ通知を購読し、フレーム精度で分岐する。
    /// </summary>
    public sealed class CriticalBranchSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("クリティカル時に再生するParticleSystemです。")]
        private ParticleSystem _criticalEffect;

        [SerializeField, Min(0f), Tooltip("クリティカル判定を待ち受ける時間です。")]
        private float _listenSeconds = 1f;

        [SerializeField, Min(0f), Tooltip("クリティカル演出を再生し続ける時間です。")]
        private float _effectDurationSeconds = 0.5f;

        /// <summary>
        ///     演出を初期状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            OnStop();
        }

        /// <summary>
        ///     待ち受け中にクリティカルが成立した場合のみ、追加演出を再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (_criticalEffect == null)
            {
                return;
            }

            _isCriticalDetected = false;
            EventBus<EOnTakeDamage>.Register(HandleTakeDamageHandler);

            try
            {
                float playbackSpeed = context.PlaybackSpeed;
                float remainingSeconds = _listenSeconds / playbackSpeed;

                // 待ち受け中にクリティカルが来たら、その時点で演出へ移る。
                while (remainingSeconds > 0f && !_isCriticalDetected)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                    remainingSeconds -= Time.deltaTime;
                }

                if (!_isCriticalDetected)
                {
                    return;
                }

                _criticalEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _criticalEffect.Play(true);
                await Awaitable.WaitForSecondsAsync(_effectDurationSeconds / playbackSpeed, cancellationToken);
            }
            finally
            {
                EventBus<EOnTakeDamage>.Unregister(HandleTakeDamageHandler);
            }
        }

        /// <summary>
        ///     追加演出を停止する。
        /// </summary>
        protected override void OnStop()
        {
            if (_criticalEffect == null)
            {
                return;
            }

            _criticalEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        ///     ダメージ通知を受け取り、クリティカル成立を記録する。
        /// </summary>
        /// <param name="damageEvent"> 受け取ったダメージ情報です。 </param>
        private void HandleTakeDamageHandler(EOnTakeDamage damageEvent)
        {
            if (!damageEvent.Critical)
            {
                return;
            }

            _isCriticalDetected = true;
        }

        private bool _isCriticalDetected;
    }
}
