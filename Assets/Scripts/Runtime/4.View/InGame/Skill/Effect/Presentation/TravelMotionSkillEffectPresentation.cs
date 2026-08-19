using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     対象のTransformをプレイヤーから対象へ向けて移動させるLitMotion演出のストラテジー。
    ///     軌跡や投射のように、2点間を移動する表現に使用する。
    /// </summary>
    public sealed class TravelMotionSkillEffectPresentation : MotionSkillEffectPresentationBase
    {
        [SerializeField, Tooltip("移動させる対象のTransformです。未設定時は自身を使用します。")]
        private Transform _travelTarget;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("移動の開始位置です。プレイヤーが0、対象が1です。")]
        private float _startRatio;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("移動の終了位置です。プレイヤーが0、対象が1です。")]
        private float _endRatio = 1f;

        [SerializeField, Tooltip("移動経路に加算するワールドオフセットです。")]
        private Vector3 _worldOffset;

        [SerializeField, Min(0f), Tooltip("移動開始までの待機時間です。")]
        private float _delaySeconds;

        [SerializeField, Min(0f), Tooltip("移動にかける時間です。")]
        private float _durationSeconds = 0.125f;

        [SerializeField, Tooltip("使用するイージングです。")]
        private Ease _ease = Ease.OutQuad;

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
        ///     プレイヤーと対象の座標から移動区間を求めてTweenを生成する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 生成したTweenのハンドルです。 </returns>
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            Vector3 startAnchor = context.PlayerTransform != null
                ? context.PlayerTransform.position
                : context.WorldPosition;

            // 対象が消滅している場合も、解決済みのワールド座標を終点として扱う。
            Vector3 endAnchor = context.HasTarget ? context.TargetTransform.position : context.WorldPosition;

            Vector3 startPosition = Vector3.Lerp(startAnchor, endAnchor, _startRatio) + _worldOffset;
            Vector3 endPosition = Vector3.Lerp(startAnchor, endAnchor, _endRatio) + _worldOffset;
            _travelTarget.position = startPosition;

            return LMotion.Create(startPosition, endPosition, _durationSeconds)
                .WithDelay(_delaySeconds)
                .WithEase(_ease)
                .BindToPosition(_travelTarget);
        }
    }
}
