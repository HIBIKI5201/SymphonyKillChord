using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
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
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }
        }

        /// <summary>
        ///     ParticleSystemを初期状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            _particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        ///     ParticleSystemを再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected override void OnPlay(in SkillEffectContext context)
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
        }

        /// <summary>
        ///     ParticleSystemを停止する。
        /// </summary>
        protected override void OnStop()
        {
            _particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        ///     パーティクルが生存しているかで再生継続を判定する。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected override bool OnCheckPlaying(float elapsedSeconds)
        {
            return _particleSystem != null && _particleSystem.IsAlive(true);
        }
    }
}
