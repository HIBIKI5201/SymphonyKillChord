using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DevelopProducts.AnimationControl
{
    public readonly struct AnimationBlendClipData
    {
        public readonly AnimationClip Clip;
        public readonly AnimationClipPlayable ClipPlayable;

        public bool IsValid => ClipPlayable.IsValid();

        public AnimationBlendClipData(
            AnimationClip clip,
            AnimationClipPlayable clipPlayable
        )
        {
            Clip = clip;
            ClipPlayable = clipPlayable;
        }
    }
}
