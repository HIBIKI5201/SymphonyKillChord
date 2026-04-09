using UnityEngine;

namespace DevelopProducts.AnimationControl.Adaptor
{
    public class AnimationAdaptor
    {
        public AnimationAdaptor(Animator animator)
        {
            _animator = animator;
        }

        public Animator Animator => _animator;
        public RuntimeAnimatorController Controller => _animator.runtimeAnimatorController;

        protected Animator _animator;
    }
}
