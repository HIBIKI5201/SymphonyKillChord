using UnityEngine;

namespace DevelopProducts.AnimationControl
{
    public readonly ref struct AnimationBlendClipRequest
    {
        public AnimationBlendClipRequest(AnimationClip clip, float enterDuration, float exitDuration)
        {
            _clip = clip;
            _enterDuration = enterDuration;
            _exitDuration = exitDuration;
        }

        public AnimationClip Clip => _clip;
        public float EnterDuration => _enterDuration;
        public float ExitDuration => _exitDuration;

        private readonly AnimationClip _clip;
        private readonly float _enterDuration;
        private readonly float _exitDuration;
    }
}
