using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;
using UnityEngine.VFX;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     VFX Graphでスキルエフェクトを再生するストラテジー。
    /// </summary>
    public sealed class VisualEffectGraphSkillEffectPresentation : SkillEffectPresentationBase
    {
        private const string DEFAULT_PLAY_EVENT_NAME = "OnPlay";

        [SerializeField, Tooltip("再生するVisual Effectです。未設定時は自身から取得します。")]
        private VisualEffect _visualEffect;

        [SerializeField, Tooltip("再生時に送るイベント名です。空欄ならPlayを呼びます。")]
        private string _playEventName = DEFAULT_PLAY_EVENT_NAME;

        [SerializeField, Tooltip("停止時に送るイベント名です。空欄ならStopを呼びます。")]
        private string _stopEventName;

        [SerializeField, Tooltip("Contextのスケール倍率を渡すExposed Property名です。空欄なら渡しません。")]
        private string _scalePropertyName;

        [SerializeField, Min(0f), Tooltip("パーティクル数による完了判定を行わない場合の固定再生時間です。0なら生存数で判定します。")]
        private float _fixedDurationSeconds;

        /// <summary>
        ///     Visual Effectの参照を解決する。
        /// </summary>
        private void Awake()
        {
            if (_visualEffect == null)
            {
                _visualEffect = GetComponent<VisualEffect>();
            }
        }

        /// <summary>
        ///     Visual Effectを初期状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            if (_visualEffect == null)
            {
                _visualEffect = GetComponent<VisualEffect>();
            }

            if (_visualEffect == null)
            {
                return;
            }

            // シェーダやグラフのコンパイルをシーンロード時に済ませ、初回再生時のヒッチを防ぐ。
            _visualEffect.Reinit();
            _visualEffect.Stop();
        }

        /// <summary>
        ///     Visual Effectを再生する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected override void OnPlay(in SkillEffectContext context)
        {
            if (_visualEffect == null)
            {
                return;
            }

            _visualEffect.Reinit();

            if (!string.IsNullOrWhiteSpace(_scalePropertyName))
            {
                _visualEffect.SetFloat(_scalePropertyName, context.Scale);
            }

            if (string.IsNullOrWhiteSpace(_playEventName))
            {
                _visualEffect.Play();
                return;
            }

            _visualEffect.SendEvent(_playEventName);
        }

        /// <summary>
        ///     Visual Effectを停止する。
        /// </summary>
        protected override void OnStop()
        {
            if (_visualEffect == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_stopEventName))
            {
                _visualEffect.Stop();
                return;
            }

            _visualEffect.SendEvent(_stopEventName);
        }

        /// <summary>
        ///     固定再生時間または生存パーティクル数で再生継続を判定する。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected override bool OnCheckPlaying(float elapsedSeconds)
        {
            if (_visualEffect == null)
            {
                return false;
            }

            if (_fixedDurationSeconds > 0f)
            {
                return elapsedSeconds < _fixedDurationSeconds;
            }

            return _visualEffect.aliveParticleCount > 0;
        }
    }
}
