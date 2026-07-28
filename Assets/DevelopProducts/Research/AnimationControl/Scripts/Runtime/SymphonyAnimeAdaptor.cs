using UnityEngine;

namespace DevelopProducts.AnimationControl.Adaptor
{
    public class SymphonyAnimeAdaptor : AnimationAdaptor
    {
        public SymphonyAnimeAdaptor(Animator animator) : base(animator)
        {
        }

        public void SetVelocity(Vector2 value)
        {
            Validate(ref value.x);
            Validate(ref value.y);

            _animator.SetFloat(_velocityHashX, value.x);
            _animator.SetFloat(_velocityHashY, value.y);
        }

        private const string PARAM_VELOCITY_X = "VelocityX";
        private const string PARAM_VELOCITY_Y = "VelocityY";

        private const float DEAD_ZONE = 0.0001f;

        private readonly int _velocityHashX = Animator.StringToHash(PARAM_VELOCITY_X);
        private readonly int _velocityHashY = Animator.StringToHash(PARAM_VELOCITY_Y);

        private void Validate(ref float value)
        {
            // NaN / Infinity 対策
            if (!float.IsFinite(value))
            {
                value = 0f;
                return;
            }

            // デッドゾーン（微小値を0に）
            if (Mathf.Abs(value) < DEAD_ZONE)
            {
                value = 0f;
                return;
            }
        }
    }
}
