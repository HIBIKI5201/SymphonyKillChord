using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     Rendererのマテリアルfloatプロパティを補間するLitMotion演出のストラテジー。
    ///     Dissolveによる実体化や消滅の表現に使用する。
    /// </summary>
    public sealed class MaterialFloatMotionSkillEffectPresentation : MotionSkillEffectPresentationBase
    {
        [SerializeField, Tooltip("対象のRendererです。未設定時は自身から取得します。")]
        private Renderer _renderer;

        [SerializeField, Tooltip("補間するマテリアルのfloatプロパティ名です。")]
        private string _propertyName = DEFAULT_PROPERTY_NAME;

        [SerializeField, Tooltip("補間の開始値です。")]
        private float _fromValue = 1f;

        [SerializeField, Tooltip("補間の終了値です。")]
        private float _toValue;

        [SerializeField, Min(0f), Tooltip("補間開始までの待機時間です。")]
        private float _delaySeconds;

        [SerializeField, Min(0f), Tooltip("補間にかける時間です。")]
        private float _durationSeconds = 0.05f;

        [SerializeField, Tooltip("使用するイージングです。")]
        private Ease _ease = Ease.Linear;

        private const string DEFAULT_PROPERTY_NAME = "_Dissolve";

        /// <summary>
        ///     Rendererとプロパティ参照を解決する。
        /// </summary>
        private void Awake()
        {
            EnsureRenderer();
        }

        /// <summary>
        ///     Rendererを解決し、開始値を適用しておく。
        /// </summary>
        protected override void OnPrewarm()
        {
            EnsureRenderer();
            ApplyValue(_fromValue);
        }

        /// <summary>
        ///     マテリアルプロパティを補間するTweenを生成する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 生成したTweenのハンドルです。 </returns>
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            ApplyValue(_fromValue);
            return LMotion.Create(_fromValue, _toValue, _durationSeconds)
                .WithDelay(_delaySeconds)
                .WithEase(_ease)
                .Bind(this, static (value, presentation) => presentation.ApplyValue(value));
        }

        /// <summary>
        ///     停止時に開始値へ戻す。
        /// </summary>
        protected override void OnRestoreState()
        {
            ApplyValue(_fromValue);
        }

        /// <summary>
        ///     Rendererの参照を必要時に解決する。
        /// </summary>
        private void EnsureRenderer()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_propertyId == 0 && !string.IsNullOrWhiteSpace(_propertyName))
            {
                _propertyId = Shader.PropertyToID(_propertyName);
            }
        }

        /// <summary>
        ///     マテリアルプロパティへ値を適用する。
        /// </summary>
        /// <param name="value"> 適用する値です。 </param>
        private void ApplyValue(float value)
        {
            if (_renderer == null)
            {
                return;
            }

            // マテリアルの複製を避けるため、MaterialPropertyBlock経由で書き換える。
            _propertyBlock ??= new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_propertyId, value);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private MaterialPropertyBlock _propertyBlock;
        private int _propertyId;
    }
}
