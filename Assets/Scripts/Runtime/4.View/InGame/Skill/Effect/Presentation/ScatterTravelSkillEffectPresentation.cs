using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     プレイヤー位置から、エフェクト原点まわりの円周上へ高速移動させるLitMotion演出のストラテジー。
    ///     プレイヤーの粒子が対象の周囲へ飛来し、その場で実体化するような表現に使用する。
    /// </summary>
    public sealed class ScatterTravelSkillEffectPresentation : MotionSkillEffectPresentationBase
    {
        [SerializeField, Tooltip("移動させる対象のTransformです。未設定時は自身を使用します。")]
        private Transform _travelTarget;

        [SerializeField, Tooltip("移動中に軌跡を描くTrailRendererです。再生開始時に軌跡を消去します。")]
        private TrailRenderer _trailRenderer;

        [SerializeField, Min(0f), Tooltip("到達点までの水平距離です。")]
        private float _ringRadius = 1.6f;

        [SerializeField, Tooltip("到達点の高さです。エフェクト原点からのローカル値で固定されます。")]
        private float _ringHeight = 1f;

        [SerializeField, Min(1), Tooltip("円周を等分する数です。重なりを避けるため個体ごとに区画を分けます。")]
        private int _sectorCount = 5;

        [SerializeField, Min(0), Tooltip("この個体が使用する区画の番号です。")]
        private int _sectorIndex;

        [SerializeField, Min(0f), Tooltip("移動開始までの待機時間です。")]
        private float _delaySeconds = 0.625f;

        [SerializeField, Min(0f), Tooltip("移動にかける時間です。")]
        private float _durationSeconds = 0.1f;

        [SerializeField, Tooltip("使用するイージングです。")]
        private Ease _ease = Ease.OutQuad;

        private const float FULL_TURN_DEGREES = 360f;

        /// <summary>
        ///     移動対象の参照を解決する。
        /// </summary>
        private void Awake()
        {
            if (_travelTarget == null)
            {
                _travelTarget = transform;
            }
        }

        /// <summary>
        ///     プレイヤー位置から円周上の到達点まで移動するTweenを生成する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 生成したTweenのハンドルです。 </returns>
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            Vector3 endLocalPosition = ResolveScatteredLocalPosition();

            // 親はエフェクト原点(対象位置)に追従するため、ローカル座標で補間して追従を保つ。
            Transform parent = _travelTarget.parent;
            Vector3 startWorldPosition = context.PlayerTransform != null
                ? context.PlayerTransform.position
                : context.WorldPosition;
            Vector3 startLocalPosition = parent != null
                ? parent.InverseTransformPoint(startWorldPosition)
                : startWorldPosition;

            _travelTarget.localPosition = startLocalPosition;

            // プールからの再利用時に、前回位置からの軌跡が残らないよう消去する。
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
            }

            return LMotion.Create(startLocalPosition, endLocalPosition, _durationSeconds)
                .WithDelay(_delaySeconds)
                .WithEase(_ease)
                .BindToLocalPosition(_travelTarget);
        }

        /// <summary>
        ///     円周上の到達点をローカル座標で求める。
        /// </summary>
        /// <returns> 到達点のローカル座標です。 </returns>
        private Vector3 ResolveScatteredLocalPosition()
        {
            // 区画ごとにランダム角を取り、ばらけさせつつ重なりを避ける。
            float sectorSize = FULL_TURN_DEGREES / _sectorCount;
            float angleDegrees = (sectorSize * _sectorIndex) + Random.Range(0f, sectorSize);
            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            return new Vector3(
                Mathf.Cos(angleRadians) * _ringRadius,
                _ringHeight,
                Mathf.Sin(angleRadians) * _ringRadius);
        }
    }
}
