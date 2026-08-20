using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     銃口から対象へ弾道を一瞬だけ描く曳光弾の演出ストラテジー。
    /// </summary>
    public sealed class TracerSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("弾道を描くLineRendererです。未設定時は自身から取得します。")]
        private LineRenderer _lineRenderer;

        [SerializeField, Tooltip("弾道の始点となる銃口のTransformです。")]
        private Transform _muzzle;

        [SerializeField, Tooltip("着弾点の高さです。対象位置からのオフセットです。")]
        private float _targetHeightOffset = 1f;

        [SerializeField, Min(0f), Tooltip("発射までの待機時間です。")]
        private float _delaySeconds = 0.86f;

        [SerializeField, Min(0f), Tooltip("弾道を表示し続ける時間です。")]
        private float _visibleSeconds = 0.04f;

        /// <summary>
        ///     LineRendererの参照を解決する。
        /// </summary>
        private void Awake()
        {
            EnsureLineRenderer();
        }

        /// <summary>
        ///     弾道を非表示にしておく。
        /// </summary>
        protected override void OnPrewarm()
        {
            EnsureLineRenderer();
            OnStop();
        }

        /// <summary>
        ///     指定時刻に銃口から対象へ弾道を描き、短時間で消す。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (_lineRenderer == null)
            {
                return;
            }

            _lineRenderer.enabled = false;

            float playbackSpeed = context.PlaybackSpeed;
            if (_delaySeconds > 0f)
            {
                await Awaitable.WaitForSecondsAsync(_delaySeconds / playbackSpeed, cancellationToken);
            }

            // 発射の瞬間に始点と終点を確定させる。
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, _muzzle != null ? _muzzle.position : transform.position);
            _lineRenderer.SetPosition(1, ResolveImpactPosition(context));
            _lineRenderer.enabled = true;

            await Awaitable.WaitForSecondsAsync(_visibleSeconds / playbackSpeed, cancellationToken);
        }

        /// <summary>
        ///     弾道を非表示にする。
        /// </summary>
        protected override void OnStop()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            _lineRenderer.enabled = false;
        }

        /// <summary>
        ///     着弾点を解決する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 着弾点のワールド座標です。 </returns>
        private Vector3 ResolveImpactPosition(SkillEffectContext context)
        {
            // 対象が消滅している場合も、解決済みのワールド座標へ撃ち込む。
            Vector3 impactPosition = context.HasTarget ? context.TargetTransform.position : context.WorldPosition;
            impactPosition.y += _targetHeightOffset;
            return impactPosition;
        }

        /// <summary>
        ///     LineRendererの参照を必要時に解決する。
        /// </summary>
        private void EnsureLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
        }
    }
}
