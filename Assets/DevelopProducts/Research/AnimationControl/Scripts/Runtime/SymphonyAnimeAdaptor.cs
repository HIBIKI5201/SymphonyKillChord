using UnityEngine;

namespace DevelopProducts.AnimationControl.Adaptor
{
    public class SymphonyAnimeAdaptor : AnimationAdaptor
    {
        public SymphonyAnimeAdaptor(Animator animator) : base(animator)
        {
        }

        public void SetVelocity(float value)
        {
            _animator.SetFloat(_velocityHash, value);
        }

        private const string PARAM_VELOCITY = "Velocity";

        private readonly int _velocityHash = Animator.StringToHash(PARAM_VELOCITY);
    }
}
