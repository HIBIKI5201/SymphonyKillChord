using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     ParticleSystemでスキルエフェクトを再生するストラテジー。
    /// </summary>
    public sealed class ParticleSystemSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("再生するParticleSystemです。未設定時は自身から取得します。")]
        private ParticleSystem _particleSystem;

        [SerializeField, Tooltip("Contextのスケール倍率をParticleSystemへ適用するかです。")]
        private bool _applyContextScale;

        /// <summary>
        ///     ParticleSystemの参照を解決する。
        /// </summary>
        private void Awake()
        {
            EnsureParticleSystem();
        }

        /// <summary>
        ///     ParticleSystemを初期状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            EnsureParticleSystem();
            OnStop();
        }

        /// <summary>
        ///     ParticleSystemを再生し、パーティクルが消滅するまで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (_particleSystem == null)
            {
                return;
            }

            if (_applyContextScale)
            {
                ParticleSystem.MainModule mainModule = _particleSystem.main;
                mainModule.startSizeMultiplier = context.Scale;
            }

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Play(true);

            // 再生開始直後は生存判定が安定しないため、1フレーム進めてから監視する。
            await Awaitable.NextFrameAsync(cancellationToken);
            while (_particleSystem != null && _particleSystem.IsAlive(true))
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        ///     ParticleSystemを停止する。
        /// </summary>
        protected override void OnStop()
        {
            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        ///     ParticleSystemの参照を必要時に解決する。
        /// </summary>
        private void EnsureParticleSystem()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }
        }
    }
}
