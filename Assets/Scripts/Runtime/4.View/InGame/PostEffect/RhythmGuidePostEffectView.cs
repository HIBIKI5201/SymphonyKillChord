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
        public void OneShotRatio(Ease ease = Ease.InCirc, float duration = 0.1f, float from = 1f)
        {
            // 再生中のアニメーションが残っていると値が競合するため、先に完了させる。
            _handle.TryComplete();
            _handle = LMotion.Create(from, 0f, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToMaterialFloat(_material, RATIO);
        }

        /// <summary> 演出色のShaderプロパティID。 </summary>
        private static readonly int COLOR = Shader.PropertyToID("_Color");

        /// <summary> 演出濃度のShaderプロパティID。 </summary>
        private static readonly int RATIO = Shader.PropertyToID("_Alpha");

        [Tooltip("フルスクリーン演出のMaterial。RendererFeatureに設定した物と同じアセットを指定")]
        [SerializeField] private Material _material;

        private float _defaultRatio;
        private MotionHandle _handle;

        /// <summary>
        ///     Materialの初期濃度を控えておく。
        /// </summary>
        private void Awake()
        {
            _defaultRatio = _material.GetFloat(RATIO);
        }

        /// <summary>
        ///     破棄時にアニメーションを停止し、Materialを初期濃度へ戻す。
        /// </summary>
        private void OnDestroy()
        {
            // Materialはアセットとして共有されるため、実行中の変更を必ず巻き戻す。
            _handle.TryCancel();
            _material.SetFloat(RATIO, _defaultRatio);
        }
    }
}
