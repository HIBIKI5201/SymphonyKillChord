using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DevelopProducts.AnimationControl
{
    public readonly struct AnimationBlendClipData
    {
        public AnimationClip Clip => _clip;
        public AnimationClipPlayable ClipPlayable => _clipPlayable;
        public float EnterDuration => _enterDuration;
        public float ExitDuration => _exitDuration;

        public bool IsValid => ClipPlayable.IsValid();

        public AnimationBlendClipData(
            AnimationBlendClipRequest request,
            AnimationClipPlayable clipPlayable)
        {
            _clip = request.Clip;
            _clipPlayable = clipPlayable;
            _enterDuration = request.EnterDuration;
            _exitDuration = request.ExitDuration;
        }

        private readonly AnimationClip _clip;
        private readonly AnimationClipPlayable _clipPlayable;
        private readonly float _enterDuration;
        private readonly float _exitDuration;
    }
}
