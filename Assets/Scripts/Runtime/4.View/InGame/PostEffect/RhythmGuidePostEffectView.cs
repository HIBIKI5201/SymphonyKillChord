using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイドの全画面演出Materialを操作するViewです。
    /// </summary>
    public sealed class RhythmGuidePostEffectView : MonoBehaviour
    {
        /// <summary>
        ///     全画面演出の色を設定する。
        /// </summary>
        /// <param name="color"> 演出へ反映する色。 </param>
        public void SetColor(Color color)
        {
            _material.SetColor(COLOR, color);
        }

        /// <summary>
        ///     指定した濃度から0へ減衰する全画面演出を一度だけ再生する。
        /// </summary>
        /// <param name="ease"> 減衰のイージング。 </param>
        /// <param name="duration"> 減衰にかける秒数。 </param>
        /// <param name="from"> 再生開始時の濃度。 </param>
        public void PlayOneShot(
            Ease ease = Ease.InCirc,
            float duration = DEFAULT_DURATION,
            float from = DEFAULT_FROM_ALPHA)
        {
            // 再生中のアニメーションが残っていると値が競合するため、先に完了させる。
            _handle.TryComplete();
            _handle = LMotion.Create(from, 0f, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToMaterialFloat(_material, ALPHA);
        }

        /// <summary> 呼び出し側が指定しない場合の減衰時間（秒）。 </summary>
        private const float DEFAULT_DURATION = 0.1f;

        /// <summary> 呼び出し側が指定しない場合の再生開始時の濃度。 </summary>
        private const float DEFAULT_FROM_ALPHA = 1f;

        /// <summary> 演出色のShaderプロパティID。 </summary>
        private static readonly int COLOR = Shader.PropertyToID("_Color");

        /// <summary> 演出濃度のShaderプロパティID。 </summary>
        private static readonly int ALPHA = Shader.PropertyToID("_Alpha");

        [Tooltip("フルスクリーン演出のMaterial。RendererFeatureに設定した物と同じアセットを指定")]
        [SerializeField] private Material _material;

        private float _defaultAlpha;
        private MotionHandle _handle;

        /// <summary>
        ///     Materialの初期濃度を控えておく。
        /// </summary>
        private void Awake()
        {
            if (_material == null)
            {
                Debug.LogError($"[{nameof(RhythmGuidePostEffectView)}] Materialが設定されていません。", this);
                return;
            }

            _defaultAlpha = _material.GetFloat(ALPHA);
        }

        /// <summary>
        ///     破棄時にアニメーションを停止し、Materialを初期濃度へ戻す。
        /// </summary>
        private void OnDestroy()
        {
            // Materialはアセットとして共有されるため、実行中の変更を必ず巻き戻す。
            _handle.TryCancel();
            _material.SetFloat(ALPHA, _defaultAlpha);
        }
    }
}
