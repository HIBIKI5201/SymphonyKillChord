using DevelopProducts.AnimationControl.Adaptor;
using DevelopProducts.AnimationControl.Blender;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.AnimationControl.Blender
{
    /// <summary>
    ///     アニメーションの操作を管理する。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private AnimationClip _playClip;
        [SerializeField]
        private Key _playKey = Key.Space;

        private Animator _animator;
        private RuntimeAnimatorController _controller;
        private AnimatorPlayableBlend _blender;
        private SymphonyAnimeAdaptor _adaptor;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = _animator.runtimeAnimatorController;
            _adaptor = new SymphonyAnimeAdaptor(_animator);
            _blender = new AnimatorPlayableBlend(_adaptor);
        }

        private void Update()
        {
            if (Keyboard.current[_playKey].wasPressedThisFrame)
            {
                _blender.Play(_playClip);
            }

            _blender?.Update();
        }

        private void OnDestroy()
        {
            _blender?.Dispose();
        }
    }
}
