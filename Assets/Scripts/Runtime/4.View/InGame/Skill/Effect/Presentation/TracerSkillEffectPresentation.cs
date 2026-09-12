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
        private const float MINIMUM_SQR_MAGNITUDE = 0.0001f;

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

        [SerializeField, Min(0f), Tooltip("着弾点を貫通して弾道を伸ばす距離です。")]
        private float _penetrationDistance = 4f;

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
            Vector3 startPosition = _muzzle != null ? _muzzle.position : transform.position;
            _lineRenderer.SetPosition(0, startPosition);
            _lineRenderer.SetPosition(1, ResolveEndPosition(startPosition, context));
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
        ///     弾道の終点を解決する。着弾点で止めず、貫通させて奥へ伸ばす。
        /// </summary>
        /// <param name="startPosition"> 弾道の始点です。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 弾道終点のワールド座標です。 </returns>
        private Vector3 ResolveEndPosition(Vector3 startPosition, SkillEffectContext context)
        {
            // 対象が消滅している場合も、解決済みのワールド座標へ撃ち込む。
            Vector3 impactPosition = context.HasTarget ? context.TargetTransform.position : context.WorldPosition;
            impactPosition.y += _targetHeightOffset;

            // 着弾点で消えると視認しづらいため、そのまま奥へ通り抜けさせる。
            Vector3 direction = impactPosition - startPosition;
            if (direction.sqrMagnitude <= MINIMUM_SQR_MAGNITUDE)
            {
                return impactPosition;
            }

            return impactPosition + (direction.normalized * _penetrationDistance);
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
