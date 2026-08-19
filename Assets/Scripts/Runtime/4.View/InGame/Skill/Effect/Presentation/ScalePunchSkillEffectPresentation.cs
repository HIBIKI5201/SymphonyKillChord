using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     対象Transformを拡大させるLitMotion演出のストラテジー。
    /// </summary>
    public sealed class ScalePunchSkillEffectPresentation : MotionSkillEffectPresentationBase
    {
        private const int PUNCH_LOOP_COUNT = 2;

        [SerializeField, Tooltip("拡大させる対象のTransformです。未設定時は自身を使用します。")]
        private Transform _scaleTarget;

        [SerializeField, Tooltip("拡大後のスケール倍率です。")]
        private float _punchScale = 1.5f;

        [SerializeField, Min(0f), Tooltip("拡大にかける時間です。")]
        private float _durationSeconds = 0.3f;

        [SerializeField, Tooltip("使用するイージングです。")]
        private Ease _ease = Ease.OutBack;

        /// <summary>
        ///     初期スケールを記録する。
        /// </summary>
        private void Awake()
        {
            if (_scaleTarget == null)
            {
                _scaleTarget = transform;
            }

            _defaultScale = _scaleTarget.localScale;
        }

        /// <summary>
        ///     拡大Tweenを生成する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 生成したTweenのハンドルです。 </returns>
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            Vector3 targetScale = _defaultScale * (_punchScale * context.Scale);
            _scaleTarget.localScale = _defaultScale;

            return LMotion.Create(_defaultScale, targetScale, _durationSeconds)
                .WithEase(_ease)
                .WithLoops(PUNCH_LOOP_COUNT, LoopType.Yoyo)
                .BindToLocalScale(_scaleTarget);
        }

        /// <summary>
        ///     スケールを初期値へ戻す。
        /// </summary>
        protected override void OnRestoreState()
        {
            if (_scaleTarget == null)
            {
                return;
            }

            _scaleTarget.localScale = _defaultScale;
        }

        private Vector3 _defaultScale = Vector3.one;
    }
}
