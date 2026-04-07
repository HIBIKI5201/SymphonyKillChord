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
            _animator.SetFloat(_velocityHashX, value.x);
            _animator.SetFloat(_velocityHashY, value.y);
        }

        private const string PARAM_VELOCITY_X = "VelocityX";
        private const string PARAM_VELOCITY_Y = "VelocityY";

        private readonly int _velocityHashX = Animator.StringToHash(PARAM_VELOCITY_X);
        private readonly int _velocityHashY = Animator.StringToHash(PARAM_VELOCITY_Y);
    }
}
