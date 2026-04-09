using DevelopProducts.AnimationControl.Adaptor;
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
        private Key _walkKey = Key.W;
        [SerializeField, Range(0, 1)]
        private float _acceleration = 0.95f;

        [SerializeField]
        private AnimationClip _playClip;
        [SerializeField]
        private Key _playKey = Key.Space;

        private Animator _animator;
        private RuntimeAnimatorController _controller;
        private AnimatorPlayableBlend _blender;
        private SymphonyAnimeAdaptor _adaptor;

        private float _velocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = _animator.runtimeAnimatorController;
            _adaptor = new SymphonyAnimeAdaptor(_animator);
            _blender = new AnimatorPlayableBlend(_adaptor);
        }

        private void Update()
        {
            float acc = 0;

            if (Keyboard.current[_walkKey].isPressed)
            {
                acc = 1;
            }

            if (Keyboard.current[_playKey].wasPressedThisFrame)
            {
                _blender.Play(_playClip);
            }

            _velocity = Mathf.Lerp(_velocity, acc, _acceleration * Time.deltaTime);

            _adaptor.SetVelocity(_velocity);
            _blender?.Update();
        }

        private void OnDestroy()
        {
            _blender?.Dispose();
        }
    }
}
