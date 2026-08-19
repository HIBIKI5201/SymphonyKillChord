using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
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

        [SerializeField, Min(0f), Tooltip("固定再生時間です。0ならパーティクルの生存数で完了を判定します。")]
        private float _fixedDurationSeconds;

        /// <summary>
        ///     Visual Effectの参照を解決する。
        /// </summary>
        private void Awake()
        {
            EnsureVisualEffect();
        }

        /// <summary>
        ///     Visual Effectを初期状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            EnsureVisualEffect();
            if (_visualEffect == null)
            {
                return;
            }

            // シェーダやグラフのコンパイルをシーンロード時に済ませ、初回再生時のヒッチを防ぐ。
            _visualEffect.Reinit();
            _visualEffect.Stop();
        }

        /// <summary>
        ///     Visual Effectを再生し、完了まで待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
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
            }
            else
            {
                _visualEffect.SendEvent(_playEventName);
            }

            if (_fixedDurationSeconds > 0f)
            {
                await Awaitable.WaitForSecondsAsync(_fixedDurationSeconds, cancellationToken);
                return;
            }

            // スポーン処理は即時に反映されないため、1フレーム進めてから生存数を監視する。
            await Awaitable.NextFrameAsync(cancellationToken);
            while (_visualEffect != null && _visualEffect.aliveParticleCount > 0)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }
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
        ///     Visual Effectの参照を必要時に解決する。
        /// </summary>
        private void EnsureVisualEffect()
        {
            if (_visualEffect == null)
            {
                _visualEffect = GetComponent<VisualEffect>();
            }
        }
    }
}
